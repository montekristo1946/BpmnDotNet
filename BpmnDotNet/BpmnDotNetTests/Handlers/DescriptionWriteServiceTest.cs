using AutoFixture;
using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Entities;
using BpmnDotNet.Handlers;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class DescriptionWriteServiceTest
{
    private readonly IFixture _fixture;
    private readonly IElasticClientSetDataAsync _elasticClient;
    private readonly ILogger<DescriptionWriteService> _logger;
    private readonly DescriptionWriteService _sut;
    
    public DescriptionWriteServiceTest()
    {
        _fixture = new Fixture();
        _elasticClient = Substitute.For<IElasticClientSetDataAsync>();
        _logger = Substitute.For<ILogger<DescriptionWriteService>>();
        _sut = new DescriptionWriteService(_elasticClient, _logger);
    }
    
    [Fact]
    public void AddDescription_WhenNewKey_ShouldAddToDictionary()
    {
        // Arrange
        var taskDefinitionId = _fixture.Create<string>();
        var description = _fixture.Create<string>();

        // Act
        _sut.AddDescription(taskDefinitionId, description);

        // Assert
        var dictionary = DescriptionWriteServiceReflectionHelper.GetDictionary(_sut);
        Assert.True(dictionary.ContainsKey(taskDefinitionId));
        Assert.Equal(description, dictionary[taskDefinitionId]);
        Assert.Single(dictionary);
    }
    
    [Fact]
    public void AddDescription_WhenExistingKey_ShouldUpdateValue()
    {
        // Arrange
        var taskDefinitionId = _fixture.Create<string>();
        var oldDescription = _fixture.Create<string>();
        var newDescription = _fixture.Create<string>();

        // Act
        _sut.AddDescription(taskDefinitionId, oldDescription);
        _sut.AddDescription(taskDefinitionId, newDescription);

        // Assert
        var dictionary = DescriptionWriteServiceReflectionHelper.GetDictionary(_sut);
        Assert.True(dictionary.ContainsKey(taskDefinitionId));
        Assert.Equal(newDescription, dictionary[taskDefinitionId]);
        Assert.NotEqual(oldDescription, dictionary[taskDefinitionId]);
        Assert.Single(dictionary); // Должен быть только один ключ
    }
    
    [Fact]
    public void AddDescription_ShouldHandleMultipleKeys()
    {
        // Arrange
        var entries = _fixture
            .CreateMany<(string Key, string Value)>(5)
            .ToArray();

        // Act
        foreach (var (key, value) in entries)
        {
            _sut.AddDescription(key, value);
        }

        // Assert
        var dictionary = DescriptionWriteServiceReflectionHelper.GetDictionary(_sut);
        Assert.Equal(5, dictionary.Count);
        
        foreach (var (key, value) in entries)
        {
            Assert.True(dictionary.ContainsKey(key));
            Assert.Equal(value, dictionary[key]);
        }
    }
    
    [Fact]
    public async Task CommitAsync_WhenDictionaryHasItems_ShouldSaveAllItems()
    {
        // Arrange
        var entries = new Dictionary<string, string>
        {
            { "task-1", "description-1" },
            { "task-2", "description-2" },
            { "task-3", "description-3" }
        };

        foreach (var entry in entries)
        {
            _sut.AddDescription(entry.Key, entry.Value);
        }

        _elasticClient
            .SetDataAsync(Arg.Any<DescriptionData>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _sut.CommitAsync();

        // Assert
        await _elasticClient
            .Received(3)
            .SetDataAsync(Arg.Any<DescriptionData>(), Arg.Any<CancellationToken>());
        
        foreach (var entry in entries)
        {
            await _elasticClient
                .Received(1)
                .SetDataAsync(
                    Arg.Is<DescriptionData>(d => 
                        d.TaskDefinitionId == entry.Key && 
                        d.Description == entry.Value),
                    Arg.Any<CancellationToken>());
        }
    }
    
    [Fact]
    public async Task CommitAsync_WhenSetDataFails_ShouldLogError()
    {
        // Arrange
        var taskDefinitionId = _fixture.Create<string>();
        var description = _fixture.Create<string>();
        
        _sut.AddDescription(taskDefinitionId, description);
        
        _elasticClient
            .SetDataAsync(Arg.Any<DescriptionData>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _sut.CommitAsync();

        // Assert
        _logger
            .Received(1)
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(taskDefinitionId)),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InitAsync_ShouldClearDictionary()
    {
        // Arrange - добавляем данные в словарь
        _sut.AddDescription("task-1", "description-1");
        _sut.AddDescription("task-2", "description-2");
        _sut.AddDescription("task-3", "description-3");
        
        // Проверяем, что данные есть
        var dictionaryBefore = DescriptionWriteServiceReflectionHelper.GetDictionary(_sut);
        Assert.Equal(3, dictionaryBefore.Count);

        // Act
        await _sut.InitAsync();

        // Assert
        var dictionaryAfter = DescriptionWriteServiceReflectionHelper.GetDictionary(_sut);
        Assert.NotNull(dictionaryAfter);
        Assert.Empty(dictionaryAfter);
    }
    
}