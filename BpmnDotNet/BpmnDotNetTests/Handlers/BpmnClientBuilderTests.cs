using AutoFixture.Kernel;
using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Elements.BpmnNatation;
using BpmnDotNet.Entities;
using BpmnDotNet.Handlers;
using BpmnDotNetTests.Utils;

namespace BpmnDotNetTests.Handlers;

using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

public class BpmnClientBuilderTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly string _tempDirectory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPathFinder _pathFinder;
    private readonly IElasticClientSetDataAsync _elasticClient;
    private readonly IHistoryNodeStateWriter _historyNodeStateWriter;
    private readonly IDescriptionWriteService _descriptionWriteService;
    private readonly IXmlSerializationProcessSection _serializerProcessSection;
    private IXmlSerializationBpmnDiagramSection _serializerDiagramSection;

    public BpmnClientBuilderTests()
    {
        _fixture = new Fixture();

        // Создаем временную директорию для тестовых .bpmn файлов
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _pathFinder = Substitute.For<IPathFinder>();
        _elasticClient = Substitute.For<IElasticClientSetDataAsync>();
        _historyNodeStateWriter = Substitute.For<IHistoryNodeStateWriter>();
        _descriptionWriteService = Substitute.For<IDescriptionWriteService>();
        _elasticClient.SetDataAsync(Arg.Any<BpmnPlane>()).Returns(Task.FromResult(true));
        _serializerProcessSection= Substitute.For<IXmlSerializationProcessSection>();
        _serializerDiagramSection = Substitute.For<IXmlSerializationBpmnDiagramSection>();
        
        var logger = Substitute.For<ILogger>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(IElement),
                typeof(SequenceFlowComponent)));
    }

    public void Dispose()
    {
        // Очищаем временную директорию после тестов
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public void Build_ShouldCreateBpmnClient_WhenValidPathAndDependencies()
    {
        // Arrange
        const int fileCount = 3;
        var bpmnFiles = BpmnClientBuilderTestHelper.CreateSampleBpmnFiles(fileCount,_tempDirectory);
    
        // Настройка для BpmnDiagramSection - разные объекты для каждого файла
        var bpmnPlanes = new List<BpmnPlane>();
        for (int i = 0; i < fileCount; i++)
        {
            bpmnPlanes.Add(_fixture.Create<BpmnPlane>());
        }
    
        var diagramCallCount = 0;
        _serializerDiagramSection.LoadXmlBpmnDiagram(Arg.Any<string>())
            .Returns(x => bpmnPlanes[diagramCallCount++]);
    
        // Настройка для BpmnProcessSection - разные DTO для каждого файла
        var processDtos = new List<BpmnProcessDto>();
        for (int i = 0; i < fileCount; i++)
        {
            processDtos.Add(_fixture.Create<BpmnProcessDto>());
        }
    
        var processCallCount = 0;
        _serializerProcessSection.LoadXmlProcessSection(Arg.Any<string>())
            .Returns(x => processDtos[processCallCount++]);
    
        // Настройка успешного ответа от ElasticClient
        _elasticClient.SetDataAsync(Arg.Any<BpmnPlane>()).Returns(Task.FromResult(true));

        // Act
        using var result = BpmnClientBuilder.Build(
            _tempDirectory,
            _loggerFactory,
            _pathFinder,
            _elasticClient,
            _historyNodeStateWriter,
            _descriptionWriteService,
            _serializerProcessSection,
            _serializerDiagramSection);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BpmnClient>(result);
    
        // Проверяем, что все файлы были загружены
        _serializerDiagramSection.Received(fileCount).LoadXmlBpmnDiagram(Arg.Any<string>());
        _serializerProcessSection.Received(fileCount).LoadXmlProcessSection(Arg.Any<string>());
    
        // Проверяем, что все plane были отправлены в Elastic
        _elasticClient.Received(fileCount).SetDataAsync(Arg.Any<BpmnPlane>());
    }
    
    [Fact]
    public async Task Build_ShouldLoadAllBpmnFiles_WhenMultipleFilesExist()
    {
        // Arrange
        const int fileCount = 5;
        var bpmnFiles = BpmnClientBuilderTestHelper.CreateSampleBpmnFiles(fileCount,_tempDirectory);
    
        var bpmnPlanes = new List<BpmnPlane>();
        var processDtos = new List<BpmnProcessDto>();
    
        for (int i = 0; i < fileCount; i++)
        {
            bpmnPlanes.Add(_fixture.Create<BpmnPlane>());
            processDtos.Add(_fixture.Create<BpmnProcessDto>());
        }
    
        var diagramCallCount = 0;
        var processCallCount = 0;
    
        _serializerDiagramSection.LoadXmlBpmnDiagram(Arg.Any<string>())
            .Returns(x => bpmnPlanes[diagramCallCount++]);
    
        _serializerProcessSection.LoadXmlProcessSection(Arg.Any<string>())
            .Returns(x => processDtos[processCallCount++]);
    
        _elasticClient.SetDataAsync(Arg.Any<BpmnPlane>()).Returns(Task.FromResult(true));

        // Act
        using var result = BpmnClientBuilder.Build(
            _tempDirectory,
            _loggerFactory,
            _pathFinder,
            _elasticClient,
            _historyNodeStateWriter,
            _descriptionWriteService,
            _serializerProcessSection,
            _serializerDiagramSection);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileCount, diagramCallCount);
        Assert.Equal(fileCount, processCallCount);
    
        await _elasticClient.Received(fileCount).SetDataAsync(Arg.Any<BpmnPlane>());
    }

    [Fact]
    public void Build_ShouldHandleEmptyDirectory_AndReturnEmptyClient()
    {
        // Arrange
        // Не создаем файлы - директория пустая
        
        // Act
        using var result = BpmnClientBuilder.Build(
            _tempDirectory,
            _loggerFactory,
            _pathFinder,
            _elasticClient,
            _historyNodeStateWriter,
            _descriptionWriteService,
            _serializerProcessSection,
            _serializerDiagramSection);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BpmnClient>(result);
    
        // Проверяем, что методы не вызывались
        _serializerDiagramSection.DidNotReceive().LoadXmlBpmnDiagram(Arg.Any<string>());
        _serializerProcessSection.DidNotReceive().LoadXmlProcessSection(Arg.Any<string>());
        _elasticClient.DidNotReceive().SetDataAsync(Arg.Any<BpmnPlane>());
    }
    
    [Fact]
    public void Build_ShouldProcessOnlyBpmnFiles_WhenOtherFilesExist()
    {
        // Arrange
        const int bpmnFileCount = 2;
        const int otherFileCount = 3;
    
        var bpmnFiles = BpmnClientBuilderTestHelper.CreateSampleBpmnFiles(bpmnFileCount,_tempDirectory);
        BpmnClientBuilderTestHelper.CreateOtherFiles(otherFileCount,_tempDirectory);
    
        var bpmnPlanes = new List<BpmnPlane>();
        var processDtos = new List<BpmnProcessDto>();
    
        for (int i = 0; i < bpmnFileCount; i++)
        {
            bpmnPlanes.Add(_fixture.Create<BpmnPlane>());
            processDtos.Add(_fixture.Create<BpmnProcessDto>());
        }
    
        var diagramCallCount = 0;
        var processCallCount = 0;
    
        _serializerDiagramSection.LoadXmlBpmnDiagram(Arg.Any<string>())
            .Returns(x => bpmnPlanes[diagramCallCount++]);
    
        _serializerProcessSection.LoadXmlProcessSection(Arg.Any<string>())
            .Returns(x => processDtos[processCallCount++]);
    
        _elasticClient.SetDataAsync(Arg.Any<BpmnPlane>()).Returns(Task.FromResult(true));

        // Act
        using var result = BpmnClientBuilder.Build(
            _tempDirectory,
            _loggerFactory,
            _pathFinder,
            _elasticClient,
            _historyNodeStateWriter,
            _descriptionWriteService,
            _serializerProcessSection,
            _serializerDiagramSection);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(bpmnFileCount, diagramCallCount);
        Assert.Equal(bpmnFileCount, processCallCount); 
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task Build_ShouldHandleDifferentFileCounts(int fileCount)
    {
        // Arrange
        var bpmnFiles = BpmnClientBuilderTestHelper.CreateSampleBpmnFiles(fileCount,_tempDirectory);
    
        var bpmnPlanes = new List<BpmnPlane>();
        var processDtos = new List<BpmnProcessDto>();
    
        for (int i = 0; i < fileCount; i++)
        {
            bpmnPlanes.Add(_fixture.Create<BpmnPlane>());
            processDtos.Add(_fixture.Create<BpmnProcessDto>());
        }
    
        var diagramCallCount = 0;
        var processCallCount = 0;
    
        _serializerDiagramSection.LoadXmlBpmnDiagram(Arg.Any<string>())
            .Returns(x => bpmnPlanes[diagramCallCount++]);
    
        _serializerProcessSection.LoadXmlProcessSection(Arg.Any<string>())
            .Returns(x => processDtos[processCallCount++]);
    
        _elasticClient.SetDataAsync(Arg.Any<BpmnPlane>()).Returns(Task.FromResult(true));

        // Act
        using var result = BpmnClientBuilder.Build(
            _tempDirectory,
            _loggerFactory,
            _pathFinder,
            _elasticClient,
            _historyNodeStateWriter,
            _descriptionWriteService,
            _serializerProcessSection,
            _serializerDiagramSection);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(fileCount, diagramCallCount);
        Assert.Equal(fileCount, processCallCount);
    
        await _elasticClient.Received(fileCount).SetDataAsync(Arg.Any<BpmnPlane>());
    }
    
    [Fact]
    public void GetAllFiles_ShouldReturnAllBpmnFiles_WhenDirectoryContainsBpmnFiles()
    {
        // Arrange
        var expectedFiles = BpmnClientBuilderTestHelper.CreateSampleBpmnFiles(3,_tempDirectory);
            
        // Act
        var result = BpmnClientBuilder.GetAllFiles(_tempDirectory);
            
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.All(result, file => Assert.EndsWith(".bpmn", file));
    }
    
    [Fact]
    public void GetAllFiles_ShouldReturnOnlyBpmnFiles_WhenMultipleFileTypesExist()
    {
        // Arrange
        BpmnClientBuilderTestHelper. CreateSampleBpmnFiles(2,_tempDirectory);
        BpmnClientBuilderTestHelper.CreateOtherFiles(3, _tempDirectory,"txt");
        BpmnClientBuilderTestHelper.CreateOtherFiles(2,_tempDirectory, "xml");
            
        // Act
        var result = BpmnClientBuilder.GetAllFiles(_tempDirectory);
            
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.All(result, file => Assert.EndsWith(".bpmn", file));
    }
    
    [Fact]
    public void GetAllFiles_ShouldReturnFiles_WhenSingleBpmnFileExists()
    {
        // Arrange
        BpmnClientBuilderTestHelper. CreateSampleBpmnFiles(1,_tempDirectory);
            
        // Act
        var result = BpmnClientBuilder.GetAllFiles(_tempDirectory);
            
        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.EndsWith(".bpmn", result[0]);
    }
    
    [Fact]
    public void GetAllFiles_ShouldThrowArgumentException_WhenPathIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            BpmnClientBuilder.GetAllFiles(null!));
            
        Assert.Equal("pathDiagram", exception.ParamName);
        Assert.Contains("[GetAllFiles] Path cannot be null or whitespace.", exception.Message);
    }
    
    [Fact]
    public void GetAllFiles_ShouldThrowArgumentException_WhenPathIsEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            BpmnClientBuilder.GetAllFiles(string.Empty));
            
        Assert.Equal("pathDiagram", exception.ParamName);
        Assert.Contains("[GetAllFiles] Path cannot be null or whitespace.", exception.Message);
    }
        
    [Fact]
    public void GetAllFiles_ShouldThrowArgumentException_WhenPathIsWhiteSpace()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            BpmnClientBuilder.GetAllFiles("   "));
            
        Assert.Equal("pathDiagram", exception.ParamName);
        Assert.Contains("[GetAllFiles] Path cannot be null or whitespace.", exception.Message);
    }
    
    [Fact]
    public void GetAllFiles_ShouldThrowDirectoryNotFoundException_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "NonExistentFolder");
            
        // Act & Assert
        var exception = Assert.Throws<DirectoryNotFoundException>(() =>
            BpmnClientBuilder.GetAllFiles(nonExistentPath));
            
        Assert.Contains($"[GetAllFiles] Directory not found: {nonExistentPath}", exception.Message);
    }
    
    [Fact]
    public void GetAllFiles_ShouldThrowInvalidOperationException_WhenNoBpmnFilesFound()
    {
        // Arrange
        BpmnClientBuilderTestHelper.CreateOtherFiles(3,_tempDirectory, "txt");
        BpmnClientBuilderTestHelper.CreateOtherFiles(2,_tempDirectory, "xml");
            
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            BpmnClientBuilder.GetAllFiles(_tempDirectory));
            
        Assert.Contains($"[GetAllFiles] Not found files in diagram {_tempDirectory}:*.bpmn.", exception.Message);
    }
        
    [Fact]
    public void GetAllFiles_ShouldThrowInvalidOperationException_WhenDirectoryIsEmpty()
    {
        // Arrange - пустая директория
            
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            BpmnClientBuilder.GetAllFiles(_tempDirectory));
            
        Assert.Contains($"[GetAllFiles] Not found files in diagram {_tempDirectory}:*.bpmn.", exception.Message);
    }
        
    [Fact]
    public void GetAllFiles_ShouldThrowInvalidOperationException_WhenOnlyNonBpmnFilesExist()
    {
        // Arrange
        BpmnClientBuilderTestHelper. CreateOtherFiles(5, _tempDirectory,"txt");
            
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            BpmnClientBuilder.GetAllFiles(_tempDirectory));
            
        Assert.Contains($"[GetAllFiles] Not found files in diagram {_tempDirectory}:*.bpmn.", exception.Message);
    }

    
}