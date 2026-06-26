using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.Dto;
using BpmnDotNet.Handlers;
using BpmnDotNet.HistoryDomain.Abstractions;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class BpmnClientTests
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHistoryNodeStateWriter _historyNodeStateWriter;
    private readonly IDescriptionWriteService _descriptionWriteService;
    private BpmnClient _bpmnClient;
    private readonly IFixture _fixture;
    private readonly IBpmnEngine _bpmnEngine;
    private readonly IProcessModelBuilder _processModelBuilder;


    public BpmnClientTests()
    {
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _bpmnEngine = Substitute.For<IBpmnEngine>();
        _historyNodeStateWriter = Substitute.For<IHistoryNodeStateWriter>();
        _descriptionWriteService = Substitute.For<IDescriptionWriteService>();
        _processModelBuilder = Substitute.For<IProcessModelBuilder>();
        _bpmnClient = new BpmnClient(
            _loggerFactory,
            _historyNodeStateWriter,
            _descriptionWriteService,
            _processModelBuilder,
            TimeSpan.FromMilliseconds(1000));
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ClearBpmnProcessesDictionaryAsync_FullPassNormalFinalizedProcess_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var bpmnEngine = Substitute.For<IBpmnEngine>();
        bpmnEngine.IsProcessCancel.Returns(true);

        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess,
            Process = bpmnEngine,
        };

        var resAdd = _bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        await _bpmnClient.ClearBpmnProcessesDictionaryAsync();

        Assert.True(resAdd);
        Assert.Empty(_bpmnClient.BpmnProcesses.Keys);

        await _bpmnClient.DisposeAsync();
    }


    [Fact]
    public async Task ClearBpmnProcessesDictionaryAsync_CheckForceCall_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var bpmnEngine = Substitute.For<IBpmnEngine>();
        bpmnEngine.IsProcessCancel.Returns(false);

        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess,
            Process = bpmnEngine,
        };

        var resAdd = _bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        await _bpmnClient.ClearBpmnProcessesDictionaryAsync(true);

        Assert.True(resAdd);
        Assert.Empty(_bpmnClient.BpmnProcesses.Keys);

        await _bpmnClient.DisposeAsync();
    }

    [Fact]
    public async Task ClearBpmnProcessesDictionaryAsync_CheckWaitPending_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var bpmnEngine = Substitute.For<IBpmnEngine>();
        bpmnEngine.IsProcessCancel.Returns(false);

        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess,
            Process = bpmnEngine,
        };

        var resAdd = _bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        await _bpmnClient.ClearBpmnProcessesDictionaryAsync();

        Assert.True(resAdd);
        Assert.Single(_bpmnClient.BpmnProcesses.Keys);

        await _bpmnClient.DisposeAsync();
    }

    [Fact]
    public void SendMessage_ShouldAddMessageToProcess_WhenProcessExists()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        var mockProcess = Substitute.For<IBpmnEngine>();
        mockProcess.AddMessageToQueue(messageType, message).Returns(true);

        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, mockProcess, _bpmnClient);

        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message);

        // Assert
        mockProcess.Received(1).AddMessageToQueue(messageType, message);
    }

    [Fact]
    public void SendMessage_ShouldWorkWithDifferentMessageTypes()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(int);
        var message = _fixture.Create<int>();

        _bpmnEngine.AddMessageToQueue(Arg.Any<Type>(), Arg.Any<object>()).Returns(true);

        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine, _bpmnClient);

        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message);

        // Assert
        _bpmnEngine.Received(1).AddMessageToQueue(messageType, message);
    }

    [Fact]
    public void SendMessage_ShouldCallAddMessageToQueue_WithCorrectParameters()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(decimal);
        var message = _fixture.Create<decimal>();


        _bpmnEngine.AddMessageToQueue(Arg.Any<Type>(), Arg.Any<object>()).Returns(true);

        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine, _bpmnClient);

        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message);

        // Assert
        _bpmnEngine.Received(1).AddMessageToQueue(
            Arg.Is<Type>(t => t == messageType),
            Arg.Is<object>(o => o.Equals(message)));
    }

    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenProcessNotFound()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Не добавляем процесс в хранилище

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));

        Assert.Contains($"[BpmnClient:SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}",
            exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenProcessIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Добавляем процесс с null значением Process
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, null, _bpmnClient);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));

        Assert.Contains($"[BpmnClient:SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}",
            exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenProcessJobStatusIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Добавляем null значение в словарь
        var processes = BpmnClientTestHelper.GetBpmnProcessesDictionary(_bpmnClient);
        processes[(idBpmnProcess, tokenProcess)] = null!;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));

        Assert.Contains($"[BpmnClient:SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}",
            exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenAddMessageToQueueReturnsFalse()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        _bpmnEngine.AddMessageToQueue(Arg.Any<Type>(), Arg.Any<object>()).Returns(false);

        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine, _bpmnClient);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));

        Assert.Contains($"[BpmnClient:SendMessage] Not Add message : {idBpmnProcess} {tokenProcess} {messageType}",
            exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenAddMessageToQueueThrowsException()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();


        _bpmnEngine.When(x => x.AddMessageToQueue(messageType, message))
            .Do(x => throw new InvalidOperationException("Test error from process"));

        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine, _bpmnClient);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));

        Assert.Contains($"Test error from process", exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldHandleDifferentProcesses_WithSameIdButDifferentTokens()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess1 = _fixture.Create<string>();
        var tokenProcess2 = _fixture.Create<string>();
        var messageType = typeof(string);
        var message1 = _fixture.Create<string>();
        var message2 = _fixture.Create<string>();

        var mockProcess1 = Substitute.For<IBpmnEngine>();
        var mockProcess2 = Substitute.For<IBpmnEngine>();

        mockProcess1.AddMessageToQueue(messageType, message1).Returns(true);
        mockProcess2.AddMessageToQueue(messageType, message2).Returns(true);

        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess1, mockProcess1, _bpmnClient);
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess2, mockProcess2, _bpmnClient);

        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess1, messageType, message1);
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess2, messageType, message2);

        // Assert
        mockProcess1.Received(1).AddMessageToQueue(messageType, message1);
        mockProcess2.Received(1).AddMessageToQueue(messageType, message2);
    }

    [Fact]
    public void SendMessage_ShouldHandleDifferentProcesses_WithDifferentIdsAndSameTokens()
    {
        // Arrange
        var idBpmnProcess1 = _fixture.Create<string>();
        var idBpmnProcess2 = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message1 = _fixture.Create<string>();
        var message2 = _fixture.Create<string>();

        var mockProcess1 = Substitute.For<IBpmnEngine>();
        var mockProcess2 = Substitute.For<IBpmnEngine>();

        mockProcess1.AddMessageToQueue(messageType, message1).Returns(true);
        mockProcess2.AddMessageToQueue(messageType, message2).Returns(true);

        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess1, tokenProcess, mockProcess1, _bpmnClient);
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess2, tokenProcess, mockProcess2, _bpmnClient);

        // Act
        _bpmnClient.SendMessage(idBpmnProcess1, tokenProcess, messageType, message1);
        _bpmnClient.SendMessage(idBpmnProcess2, tokenProcess, messageType, message2);

        // Assert
        mockProcess1.Received(1).AddMessageToQueue(messageType, message1);
        mockProcess2.Received(1).AddMessageToQueue(messageType, message2);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenIdBpmnProcessIsNull()
    {
        // Arrange
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(null!, tokenProcess, messageType, message));

        Assert.Contains("idBpmnProcess", exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenIdBpmnProcessIsEmpty()
    {
        // Arrange
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage(string.Empty, tokenProcess, messageType, message));

        Assert.Contains("idBpmnProcess", exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenIdBpmnProcessIsWhiteSpace()
    {
        // Arrange
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage("   ", tokenProcess, messageType, message));

        Assert.Contains("idBpmnProcess", exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenTokenProcessIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, null!, messageType, message));

        Assert.Contains("tokenProcess", exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenTokenProcessIsEmpty()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, string.Empty, messageType, message));

        Assert.Contains("tokenProcess", exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenTokenProcessIsWhiteSpace()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, "   ", messageType, message));

        Assert.Contains("tokenProcess", exception.Message);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentNullException_WhenMessageTypeIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var message = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, null!, message));

        Assert.Equal("messageType", exception.ParamName);
    }

    [Fact]
    public void SendMessage_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, null!));

        Assert.Equal("message", exception.ParamName);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task RegisterHandlers_ShouldRegisterHandlers_WhenHandlersAreValid(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder,
        [Frozen] ILogger<BpmnClient> logger)
    {
        // Тест проверяет, что метод RegisterHandlers успешно регистрирует обработчики
        // Arrange
        loggerFactory.CreateLogger<BpmnClient>().Returns(logger);
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));

        var handler1 = Substitute.For<IBpmnHandler>();
        handler1.TaskDefinitionId.Returns("Handler1");
        handler1.Description.Returns("Description1");
        handler1.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler2 = Substitute.For<IBpmnHandler>();
        handler2.TaskDefinitionId.Returns("Handler2");
        handler2.Description.Returns("Description2");
        handler2.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handlers = new IBpmnHandler[] { handler1, handler2 };

        descriptionWriteService.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        bpmnClient.RegisterHandlers(handlers);

        // Assert
        descriptionWriteService.Received(1).InitNewInstance();
        descriptionWriteService.Received(1).AddDescription("Handler1", "Description1");
        descriptionWriteService.Received(1).AddDescription("Handler2", "Description2");
        await descriptionWriteService.Received(1).CommitAsync(Arg.Any<CancellationToken>());

        logger.Received(2).Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[BpmnClient:RegisterHandlers] Registration completed;")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()!);
        
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void RegisterHandlers_ShouldThrowArgumentNullException_WhenHandlersIsNull(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод выбрасывает ArgumentNullException, когда handlers равен null
        // Arrange
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => bpmnClient.RegisterHandlers<IBpmnHandler>(null!));
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void RegisterHandlers_ShouldThrowInvalidOperationException_WhenTaskDefinitionIdIsNull(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод выбрасывает InvalidOperationException, когда TaskDefinitionId равен null
        // Arrange
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        var handler = Substitute.For<IBpmnHandler>();
        handler.TaskDefinitionId.Returns((string)null!);
        handler.Description.Returns("Description");
        
        var handlers = new IBpmnHandler[] { handler };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            bpmnClient.RegisterHandlers(handlers));
        
        Assert.Contains("TaskDefinitionId is null", exception.Message);
    }
    
    
    [Theory]
    [AutoNSubstituteData]
    internal void RegisterHandlers_ShouldThrowInvalidOperationException_WhenDescriptionIsNull(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод выбрасывает InvalidOperationException, когда Description равен null
        // Arrange
     
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        var handler = Substitute.For<IBpmnHandler>();
        handler.TaskDefinitionId.Returns("Handler1");
        handler.Description.Returns((string)null!);
        
        var handlers = new IBpmnHandler[] { handler };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            bpmnClient.RegisterHandlers(handlers));
        
        Assert.Contains("Description is null", exception.Message);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void RegisterHandlers_ShouldThrowInvalidOperationException_WhenHandlerAlreadyRegistered(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод выбрасывает InvalidOperationException, когда обработчик уже зарегистрирован
        // Arrange
        
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        var handler1 = Substitute.For<IBpmnHandler>();
        handler1.TaskDefinitionId.Returns("Handler1");
        handler1.Description.Returns("Description1");
        handler1.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        var handler2 = Substitute.For<IBpmnHandler>();
        handler2.TaskDefinitionId.Returns("Handler1"); // Такой же ID
        handler2.Description.Returns("Description2");
        handler2.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        var handlers = new IBpmnHandler[] { handler1, handler2 };
        
        descriptionWriteService.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            bpmnClient.RegisterHandlers(handlers));
        
        Assert.Contains("is already registered", exception.Message);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void RegisterHandlers_ShouldCallInitNewInstance_OnDescriptionWriteService(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод вызывает InitNewInstance на DescriptionWriteService
        // Arrange
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        var handler = Substitute.For<IBpmnHandler>();
        handler.TaskDefinitionId.Returns("Handler1");
        handler.Description.Returns("Description1");
        handler.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        var handlers = new IBpmnHandler[] { handler };
        
        descriptionWriteService.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        bpmnClient.RegisterHandlers(handlers);

        // Assert
        descriptionWriteService.Received(1).InitNewInstance();
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void RegisterHandlers_ShouldCallAddDescription_ForEachHandler(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод вызывает AddDescription для каждого обработчика
        // Arrange
 
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        var handler1 = Substitute.For<IBpmnHandler>();
        handler1.TaskDefinitionId.Returns("Handler1");
        handler1.Description.Returns("Description1");
        handler1.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        var handler2 = Substitute.For<IBpmnHandler>();
        handler2.TaskDefinitionId.Returns("Handler2");
        handler2.Description.Returns("Description2");
        handler2.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        var handlers = new IBpmnHandler[] { handler1, handler2 };
        
        descriptionWriteService.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        bpmnClient.RegisterHandlers(handlers);

        // Assert
        descriptionWriteService.Received(1).AddDescription("Handler1", "Description1");
        descriptionWriteService.Received(1).AddDescription("Handler2", "Description2");
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task CleaningBpmnProcesses_ShouldRemoveCompletedProcesses_WhenIsProcessCancelIsTrue(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод CleaningBpmnProcesses удаляет завершенные процессы из словаря
        // Arrange
        
        var bpmnClient = Substitute.ForPartsOf<BpmnClient>(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        bpmnClient.ClearBpmnProcessesDictionaryAsync(Arg.Any<bool>()).Returns(Task.CompletedTask);
        
        var processMock = Substitute.For<IBpmnEngine>();
        processMock.IsProcessCancel.Returns(true);
        
        var businessProcessJobStatus = new BusinessProcessJobStatus
        {
            IdBpmnProcess = "TestProcess",
            TokenProcess = "TestToken",
            Process = processMock
        };
        
        bpmnClient.BpmnProcesses.TryAdd(("TestProcess", "TestToken"), businessProcessJobStatus);
        
        var cts =  new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        var delay = TimeSpan.FromMilliseconds(100);

        // Act
        await bpmnClient.CleaningBpmnProcesses(cts.Token, delay);

        // Assert
        await bpmnClient.Received().ClearBpmnProcessesDictionaryAsync(Arg.Any<bool>());

    }
    
    
    [Theory]
    [AutoNSubstituteData]
    internal void FillingBusinessProcessDtos_ShouldAddProcesses_WhenNoDuplicates(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder,
        [Frozen] ILogger<BpmnClient> logger)
    {
        // Тест проверяет, что метод FillingBusinessProcessDtos успешно добавляет процессы в кэш
        // Arrange
        var elements = new IElement[] { };
        var businessProcessDtos = new BpmnProcessDto[]
        {
            new BpmnProcessDto("Process1", elements),
            new BpmnProcessDto("Process2", elements),
            new BpmnProcessDto("Process3", elements)
        };
        
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        bpmnClient.FillingBusinessProcessDtos(businessProcessDtos);

        // Act & Assert
        // Проверяем через рефлексию, что процессы добавлены в _bpmnProcessDtos
        var bpmnProcessDtosField = typeof(BpmnClient).GetField("_bpmnProcessDtos", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(bpmnProcessDtosField);
        var bpmnProcessDtos = (ConcurrentDictionary<string, BpmnProcessDto>)bpmnProcessDtosField.GetValue(bpmnClient)!;
        
        Assert.NotNull(bpmnProcessDtos);
        
        Assert.Equal(3, bpmnProcessDtos.Count);
        Assert.True(bpmnProcessDtos.ContainsKey("Process1"));
        Assert.True(bpmnProcessDtos.ContainsKey("Process2"));
        Assert.True(bpmnProcessDtos.ContainsKey("Process3"));
        
        logger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to load process")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()!);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void FillingBusinessProcessDtos_ShouldThrowInvalidOperationException_WhenDuplicateKeyExists(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод FillingBusinessProcessDtos выбрасывает исключение при дубликате ключа
        // Arrange
        var elements = new IElement[] { };
        var businessProcessDtos = new[]
        {
            new BpmnProcessDto("Process1", elements),
            new BpmnProcessDto("Process1", elements) // Дубликат
        };

        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
                bpmnClient.FillingBusinessProcessDtos(businessProcessDtos));

        Assert.Contains("Fail, duplicate key load", exception.Message);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void GetBpmnShema_ShouldReturnBpmnProcessDto_WhenKeyExists(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод GetBpmnShema возвращает BpmnProcessDto, когда ключ существует
        // Arrange
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        var elements = new IElement[] { };
        var businessProcessDtos = new BpmnProcessDto[]
        {
            new BpmnProcessDto("TestProcess", elements),
     
        };
        bpmnClient.FillingBusinessProcessDtos(businessProcessDtos);
        
        var bpmnProcessDtosField = typeof(BpmnClient).GetField("_bpmnProcessDtos", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(bpmnProcessDtosField);
        var bpmnProcessDtos = (ConcurrentDictionary<string, BpmnProcessDto>)bpmnProcessDtosField.GetValue(bpmnClient)!;
        Assert.NotNull(bpmnProcessDtos);

        // Act
        var result = bpmnClient.GetBpmnShema(bpmnProcessDtos, "TestProcess");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TestProcess", result.IdBpmnProcess);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void GetBpmnShema_ShouldThrowInvalidOperationException_WhenKeyDoesNotExist(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter,
        [Frozen] IDescriptionWriteService descriptionWriteService,
        [Frozen] IProcessModelBuilder processModelBuilder)
    {
        // Тест проверяет, что метод GetBpmnShema выбрасывает InvalidOperationException, когда ключ не существует
        // Arrange
        var bpmnClient = new BpmnClient(
            loggerFactory,
            historyNodeStateWriter,
            descriptionWriteService,
            processModelBuilder,
            TimeSpan.FromSeconds(1));
        
        var bpmnProcessDtosField = typeof(BpmnClient).GetField("_bpmnProcessDtos", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(bpmnProcessDtosField);
        var bpmnProcessDtos = (ConcurrentDictionary<string, BpmnProcessDto>)bpmnProcessDtosField.GetValue(bpmnClient)!;
        Assert.NotNull(bpmnProcessDtos);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            bpmnClient.GetBpmnShema(bpmnProcessDtos, "NonExistentProcess"));
        
        Assert.Contains("Not find BpmnProcessDto", exception.Message);
        Assert.Contains("NonExistentProcess", exception.Message);
    }
    
}