using System.Collections.Concurrent;
using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.BpmnEngineDomain.Handlers;
using BpmnDotNet.Handlers;
using BpmnDotNet.HistoryDomain.Abstractions;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.BpmnEngineDomain;

public class BpmnEngineTest
{
    private readonly IXmlSerializationProcessSection _xmlSerializationProcessSection;
    private readonly ProcessModelBuilder _processModelBuilder;
    private readonly BpmnEngine _bpmnEngine;
    private readonly ILogger<BpmnEngine> _logger;
    private readonly Fixture _fixture;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IHistoryNodeStateWriter _historyNodeStateWriter;

    public BpmnEngineTest()
    {
        _xmlSerializationProcessSection = new XmlSerializationProcessSection();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _processModelBuilder = new ProcessModelBuilder(_loggerFactory);
        _logger = Substitute.For<ILogger<BpmnEngine>>();
        _historyNodeStateWriter = Substitute.For<IHistoryNodeStateWriter>();
        _bpmnEngine = new BpmnEngine(_logger, _historyNodeStateWriter);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task StartProcessAsync_CheckBaseBpmnProcess_CallMethod()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_3.bpmn");
        var count = 0;
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>
        {
            ["StartEvent_01"] = (ctx, ct) =>
            {
                count++;
                return Task.CompletedTask;
            },
            ["Activity_01"] = (ctx, ct) =>
            {
                count++;
                return Task.CompletedTask;
            },
            ["Event_01"] = (ctx, ct) =>
            {
                count++;
                return Task.CompletedTask;
            },
        };
        var processModel = _processModelBuilder.Build(diagram, handlers);
        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await _bpmnEngine.StartProcessAsync(contextBpmnProcess, processModel, cancellationToken.Token);

        await res.ProcessTask.WaitAsync(cancellationToken.Token);

        Assert.Equal(3, count);
    }

    [Fact]
    public void CreateStartToken_WithStartServiceTask_EnqueuesStartToken()
    {
        var logger = Substitute.For<ILogger<StartEvent>>();
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var startTask = new StartEvent(logger, handler, "StartEvent_01");
        var serviceTask = Substitute.For<ServiceTask>(
            Substitute.For<ILogger<ServiceTask>>(),
            handler, _fixture.Create<string>());

        var endEvent = Substitute.For<EndEvent>(
            Substitute.For<ILogger<EndEvent>>(),
            handler, _fixture.Create<string>());

        var processModel = new ProcessModel
        {
            Nodes = new ConcurrentDictionary<string, IBpmnNode>
            {
                [serviceTask.Id] = serviceTask,
                [startTask.Id] = startTask,
                [endEvent.Id] = endEvent,
            },
        };

        _bpmnEngine.CreateStartToken(processModel);

        var eventQueueField =
            typeof(BpmnEngine).GetField("_eventQueue", BindingFlags.Instance | BindingFlags.NonPublic);
        var eventQueue = (ConcurrentQueue<Token>)eventQueueField!.GetValue(_bpmnEngine)!;

        Assert.True(eventQueue.TryPeek(out var token));
        Assert.Equal(startTask.Id, token.CurrentNodeId);
    }

    [Fact]
    public void CreateStartToken_WithoutStartEvent_ThrowsInvalidOperationException()
    {
        var processModel = new ProcessModel();

        var exception = Assert.Throws<InvalidOperationException>(() => _bpmnEngine.CreateStartToken(processModel));

        Assert.Contains("No ServiceTask found", exception.Message);
    }

    [Theory]
    [InlineData("StartEvent_1")]
    [InlineData("ServiceTaskFirstHandler")]
    [InlineData("GatewayFirstHandler")]
    [InlineData("SendTaskFirstHandler")]
    [InlineData("Gateway_Second")]
    [InlineData("GatewayThirdHandler")]
    [InlineData("ServiceTaskSecondHandler")]
    [InlineData("ServiceTaskThirdHandler")]
    [InlineData("ServiceTaskFourthHandler")]
    [InlineData("SubProcessFirstHandler")]
    [InlineData("GatewayFourthHandler")]
    [InlineData("End_event")]
    public async Task StartProcessAsync_CheckStartEvent_1_CallMethod(string idActivity)
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_1.bpmn");
        var isCallMethod = false;
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>
        {
            [idActivity] = (ctx, ct) =>
            {
                isCallMethod = true;
                return Task.CompletedTask;
            },
        };
        var processModel = _processModelBuilder.Build(diagram, handlers);
        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(500));
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();
        var conditionRoute = new ConcurrentDictionary<string, string>();
        conditionRoute.TryAdd("GatewayFirstHandler", "Flow_in_SendTaskFirstHandler");

        contextBpmnProcess.ConditionRoute.Returns(conditionRoute);
        var res = await _bpmnEngine.StartProcessAsync(contextBpmnProcess, processModel, cancellationToken.Token);

        await res.ProcessTask.WaitAsync(cancellationToken.Token);

        Assert.True(isCallMethod);
    }

    [Fact]
    public async Task StartProcessAsync_CheckReceiveTask_CallMethod()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_1.bpmn");
        var isCallMethod = false;
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>
        {
            ["ReceiveTaskFirstHandle"] = (ctx, ct) =>
            {
                isCallMethod = true;
                return Task.CompletedTask;
            },
        };
        var processModel = _processModelBuilder.Build(diagram, handlers);
        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(500));
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();
        var conditionRoute = new ConcurrentDictionary<string, string>();
        conditionRoute.TryAdd("GatewayFirstHandler", "Flow_in_ReceiveTaskFirstHandle");

        var receivedMessage = new ConcurrentDictionary<string, object>();
        receivedMessage["ReceiveTaskFirstHandle"] = new TaskCompletionSource<object>();

        contextBpmnProcess.ConditionRoute.Returns(conditionRoute);
        contextBpmnProcess.ReceivedMessage.Returns(receivedMessage);
        var res = await _bpmnEngine.StartProcessAsync(contextBpmnProcess, processModel, cancellationToken.Token);

        await res.ProcessTask.WaitAsync(cancellationToken.Token);

        Assert.True(isCallMethod);
    }


    [Theory]
    [InlineData("ExpectedNodeId")]
    [InlineData("AnotherNodeId")]
    public void GetIdNodeReceiveMessage_WhenMessageTypeExists_ReturnsIdNode(string expectedId)
    {
        // Arrange
        var messageType = typeof(string);
        var context = Substitute.For<IContextBpmnProcess>();

        var dictionary = new ConcurrentDictionary<Type, string>();
        dictionary.TryAdd(messageType, expectedId);

        context.RegistrationMessagesType.Returns(dictionary);
        context.IdBpmnProcess.Returns(_fixture.Create<string>());
        context.TokenProcess.Returns(_fixture.Create<string>());

        var sut = Substitute.ForPartsOf<BpmnEngine>(
            Substitute.For<ILogger<BpmnEngine>>(),
            Substitute.For<IHistoryNodeStateWriter>());
        // Act
        var result = sut.GetIdNodeReceiveMessage(context, messageType);

        // Assert
        Assert.Equal(expectedId, result);
    }

    [Fact]
    public void GetIdNodeReceiveMessage_WhenRegistrationMessagesTypeIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var messageType = typeof(string);
        var context = Substitute.For<IContextBpmnProcess>();

        context.RegistrationMessagesType.Returns((ConcurrentDictionary<Type, string>)null!);
        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        var sut = Substitute.ForPartsOf<BpmnEngine>(
            Substitute.For<ILogger<BpmnEngine>>(),
            Substitute.For<IHistoryNodeStateWriter>());
        // Act & Assert
        var exception =
            Assert.Throws<InvalidOperationException>(() => sut.GetIdNodeReceiveMessage(context, messageType));

        Assert.Contains("Not find registrationMessagesType dictionary", exception.Message);
        Assert.Contains(messageType.ToString(), exception.Message);
        Assert.Contains("TestProcessId", exception.Message);
        Assert.Contains("TestToken", exception.Message);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, "")]
    [InlineData(false, " ")]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, " ")]
    public void GetIdNodeReceiveMessage_WhenMessageTypeNotFoundOrIdNodeInvalid_ThrowsInvalidOperationException(
        bool addOtherType,
        string? invalidIdNode)
    {
        // Arrange
        var messageType = typeof(string);
        var context = Substitute.For<IContextBpmnProcess>();

        var dictionary = new ConcurrentDictionary<Type, string?>();

        if (addOtherType)
        {
            dictionary.TryAdd(typeof(int), invalidIdNode);
        }

        if (invalidIdNode != null)
        {
            dictionary.TryAdd(messageType, invalidIdNode);
        }

        context.RegistrationMessagesType.Returns(dictionary!);
        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        var sut = Substitute.ForPartsOf<BpmnEngine>(
            Substitute.For<ILogger<BpmnEngine>>(),
            Substitute.For<IHistoryNodeStateWriter>());
        // Act & Assert
        var exception =
            Assert.Throws<InvalidOperationException>(() => sut.GetIdNodeReceiveMessage(context, messageType));

        Assert.Contains("Not find registration messages type", exception.Message);
        Assert.Contains(messageType.ToString(), exception.Message);
        Assert.Contains("TestProcessId", exception.Message);
        Assert.Contains("TestToken", exception.Message);
    }

    [Theory]
    [InlineData("NodeId1")]
    [InlineData("NodeId2")]
    public void AddMessageToQueue_WhenReceivedMessageExists_AddsMessageToDictionary(string idNode)
    {
        // Arrange
        var message = new { Data = "test message" };
        var context = Substitute.For<IContextBpmnProcess>();

        var dictionary = new ConcurrentDictionary<string, object>();

        context.ReceivedMessage.Returns(dictionary);
        context.IdBpmnProcess.Returns(_fixture.Create<string>());
        context.TokenProcess.Returns(_fixture.Create<string>());

        var sut = Substitute.ForPartsOf<BpmnEngine>(
            Substitute.For<ILogger<BpmnEngine>>(),
            Substitute.For<IHistoryNodeStateWriter>());
        // Act
        sut.AddMessageToQueue(idNode, message, context);

        // Assert
        Assert.True(dictionary.ContainsKey(idNode));
        Assert.Equal(message, dictionary[idNode]);
    }

    [Fact]
    public void AddMessageToQueue_WhenReceivedMessageIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var idNode = "TestNodeId";
        var message = new { Data = "test message" };
        var context = Substitute.For<IContextBpmnProcess>();

        context.ReceivedMessage.Returns((ConcurrentDictionary<string, object>)null!);
        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        var sut = Substitute.ForPartsOf<BpmnEngine>(
            Substitute.For<ILogger<BpmnEngine>>(),
            Substitute.For<IHistoryNodeStateWriter>());
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => sut.AddMessageToQueue(idNode, message, context));

        Assert.Contains("[BpmnEngine:AddMessageToQueue] Not find ReceivedMessage dictionary", exception.Message);
        Assert.Contains("TestProcessId", exception.Message);
        Assert.Contains("TestToken", exception.Message);
    }


    [Fact]
    public void AddMessageToQueue_FullPass_CountSemaphoreCall()
    {
        // Arrange
        var message = new { Data = "test message" };
        var sut = Substitute.ForPartsOf<BpmnEngine>(
            Substitute.For<ILogger<BpmnEngine>>(),
            Substitute.For<IHistoryNodeStateWriter>());
        var semaphoreField = typeof(BpmnEngine).GetField("_semaphore",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var semaphore = (SemaphoreSlim)semaphoreField!.GetValue(sut);
        Assert.NotNull(semaphore);

        var context = Substitute.For<IContextBpmnProcess>();

        var contextField = typeof(BpmnEngine).GetField("_contextBpmnProcess",
            BindingFlags.NonPublic | BindingFlags.Instance);
        contextField?.SetValue(sut, context);
        Assert.NotNull(context);
        sut.GetIdNodeReceiveMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<Type>()).Returns(_fixture.Create<string>());
        sut.AddMessageToQueue(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<IContextBpmnProcess>()).Returns(true);

        // Act
        var result = sut.AddMessageToQueue(typeof(string), message);

        // Assert
        Assert.True(result);
        Assert.Equal(1, semaphore.CurrentCount); // Семафор был освобожден
        sut.Received(1).GetIdNodeReceiveMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<Type>());
        sut.Received(1).AddMessageToQueue(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<IContextBpmnProcess>());
    }

    [Fact]
    public void AddMessageToQueue_CheckNextToken_CountToken()
    {
        // Arrange
        var idNode = "TestNodeId";
        var message = new { Data = "test message" };
        var sut = Substitute.ForPartsOf<BpmnEngine>(
            Substitute.For<ILogger<BpmnEngine>>(),
            Substitute.For<IHistoryNodeStateWriter>());
        var semaphoreField = typeof(BpmnEngine).GetField("_semaphore",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var semaphore = (SemaphoreSlim)semaphoreField!.GetValue(sut);
        Assert.NotNull(semaphore);

        var context = Substitute.For<IContextBpmnProcess>();

        var contextField = typeof(BpmnEngine).GetField("_contextBpmnProcess",
            BindingFlags.NonPublic | BindingFlags.Instance);
        contextField?.SetValue(sut, context);
        Assert.NotNull(context);
        sut.GetIdNodeReceiveMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<Type>()).Returns(idNode);
        sut.AddMessageToQueue(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<IContextBpmnProcess>()).Returns(true);


        var eventQueueField = typeof(BpmnEngine).GetField("_eventQueue",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(eventQueueField);
        var eventQueue = (ConcurrentQueue<Token>)eventQueueField.GetValue(sut)!;
        Assert.NotNull(eventQueue);

        // Act
        _ = sut.AddMessageToQueue(typeof(string), message);

        // Assert
        Assert.Single(eventQueue);
        Assert.Equal(idNode, eventQueue.First().CurrentNodeId);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ThreadBackground_ShouldCompleteSuccessfully_WhenProcessCompletes(
        [Frozen] ILogger<BpmnEngine> logger,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter)
    {
        // Тест проверяет, что метод ThreadBackground успешно завершается, когда процесс выполнен
        // Arrange
        var engine = Substitute.ForPartsOf<BpmnEngine>(logger, historyNodeStateWriter);

        var context = Substitute.For<IContextBpmnProcess>();
        context.IdBpmnProcess.Returns("TestProcess");
        context.TokenProcess.Returns("TestToken");

        var processModel = new ProcessModel
        {
            Nodes = new(),
        };

        var cts = new CancellationTokenSource();

        // Настраиваем RunEventLoopAsync для возврата true (процесс завершен)
        engine.RunEventLoopAsync(
                Arg.Any<IContextBpmnProcess>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ConcurrentDictionary<string, string>>(),
                Arg.Any<ConcurrentQueue<Token>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Освобождаем семафор, чтобы ThreadBackground мог начать выполнение
        var semaphoreField = typeof(BpmnEngine).GetField("_semaphore",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(semaphoreField);
        var semaphore = (SemaphoreSlim)semaphoreField.GetValue(engine)!;
        Assert.NotNull(semaphore);

        semaphore.Release();
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Act
        await engine.ThreadBackground(processModel, context, startSignal,cts.Token);

        // Assert
        await engine.Received(1).RunEventLoopAsync(
            Arg.Is<IContextBpmnProcess>(c => c == context),
            Arg.Is<ProcessModel>(p => p == processModel),
            Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
            Arg.Any<ConcurrentDictionary<string, string>>(),
            Arg.Any<ConcurrentQueue<Token>>(),
            Arg.Is<CancellationToken>(ct => ct == cts.Token));

        await historyNodeStateWriter.Received(1).SetStateProcessAsync(
            Arg.Is<string>(id => id == "TestProcess"),
            Arg.Is<string>(token => token == "TestToken"),
            Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
            Arg.Any<ConcurrentDictionary<string, string>>(),
            Arg.Any<long>());

        Assert.True(engine.IsProcessCancel);

        logger.Received(1).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Starting business process")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()!);

        logger.Received(1).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("End business")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()!);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ThreadBackground_ShouldHandleGeneralException(
        [Frozen] ILogger<BpmnEngine> logger,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter)
    {
        // Тест проверяет, что метод ThreadBackground корректно обрабатывает общие исключения
        // Arrange
        var engine = Substitute.ForPartsOf<BpmnEngine>(logger, historyNodeStateWriter);

        var context = Substitute.For<IContextBpmnProcess>();
        context.IdBpmnProcess.Returns("TestProcess");
        context.TokenProcess.Returns("TestToken");

        var processModel = new ProcessModel
        {
            Nodes = new ConcurrentDictionary<string, IBpmnNode>()
        };

        var cts = new CancellationTokenSource();
        var expectedException = new InvalidOperationException("Test exception");

        engine.When(x => x.RunEventLoopAsync(
                Arg.Any<IContextBpmnProcess>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ConcurrentDictionary<string, string>>(),
                Arg.Any<ConcurrentQueue<Token>>(),
                Arg.Any<CancellationToken>()))
            .Do(x => throw expectedException);

        // Освобождаем семафор, чтобы ThreadBackground мог начать выполнение
        var semaphoreField = typeof(BpmnEngine).GetField("_semaphore",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(semaphoreField);
        var semaphore = (SemaphoreSlim)semaphoreField.GetValue(engine)!;
        Assert.NotNull(semaphore);
        semaphore.Release();
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Act
        await engine.ThreadBackground(processModel, context,startSignal, cts.Token);

        // Assert
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Exception")),
            Arg.Is<Exception>(ex => ex == expectedException),
            Arg.Any<Func<object, Exception, string>>()!);

        await historyNodeStateWriter.Received(1).SetStateProcessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
            Arg.Any<ConcurrentDictionary<string, string>>(),
            Arg.Any<long>());

        Assert.True(engine.IsProcessCancel);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task RunEventLoopAsync_ShouldComplete_WhenAllNodesExecuted(
        [Frozen] ILogger<BpmnEngine> logger,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter)
    {
        // Тест проверяет, что RunEventLoopAsync успешно завершается, когда все ноды выполнены
        // Arrange
        var engine = Substitute.ForPartsOf<BpmnEngine>(logger, historyNodeStateWriter);

        var context = Substitute.For<IContextBpmnProcess>();
        context.IdBpmnProcess.Returns("TestProcess");
        context.TokenProcess.Returns("TestToken");

        var node = Substitute.For<IBpmnNode>();
        node.Id.Returns("Node1");
        node.ExecuteAsync(
                Arg.Any<ProcessModel>(),
                Arg.Any<IContextBpmnProcess>(),
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ConcurrentDictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BpmnNodeResult
            {
                Status = StatusNode.AllBpmnProcessCompleted,
                Tokens = new List<Token>()
            }));

        var processModel = new ProcessModel();
        processModel.Nodes["Node1"] = node;

        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();
        var eventQueue = new ConcurrentQueue<Token>();
        eventQueue.Enqueue(new Token { CurrentNodeId = "Node1" });

        var cts = CancellationToken.None;

        // Act
        var result = await engine.RunEventLoopAsync(
            context,
            processModel,
            nodeStateRegistry,
            errorRegistry,
            eventQueue,
            cts);

        // Assert
        Assert.True(result);

        await node.Received(1).ExecuteAsync(
            Arg.Is<ProcessModel>(p => p == processModel),
            Arg.Is<IContextBpmnProcess>(c => c == context),
            Arg.Is<ConcurrentDictionary<string, StatusNode>>(d => d == nodeStateRegistry),
            Arg.Is<ConcurrentDictionary<string, string>>(d => d == errorRegistry),
            Arg.Is<CancellationToken>(ct => ct == cts));

        Assert.True(nodeStateRegistry.ContainsKey("Node1"));
        Assert.Equal(StatusNode.Works, nodeStateRegistry["Node1"]);

        await historyNodeStateWriter.Received(2).SetStateProcessAsync(
            Arg.Is<string>(id => id == "TestProcess"),
            Arg.Is<string>(token => token == "TestToken"),
            Arg.Is<ConcurrentDictionary<string, StatusNode>>(d => d == nodeStateRegistry),
            Arg.Is<ConcurrentDictionary<string, string>>(d => d == errorRegistry),
            Arg.Any<long>());
    }


    [Theory]
    [AutoNSubstituteData]
    internal async Task RunEventLoopAsync_ShouldReturnFalse_WhenQueueIsEmpty(
        [Frozen] ILogger<BpmnEngine> logger,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter)
    {
        // Тест проверяет, что RunEventLoopAsync возвращает false, когда очередь пуста
        // Arrange
        var engine = Substitute.ForPartsOf<BpmnEngine>(logger, historyNodeStateWriter);

        var context = Substitute.For<IContextBpmnProcess>();
        var processModel = new ProcessModel();
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();
        var eventQueue = new ConcurrentQueue<Token>();
        var cts = CancellationToken.None;

        // Act
        var result = await engine.RunEventLoopAsync(
            context,
            processModel,
            nodeStateRegistry,
            errorRegistry,
            eventQueue,
            cts);

        // Assert
        Assert.False(result);
        await historyNodeStateWriter.DidNotReceive().SetStateProcessAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
            Arg.Any<ConcurrentDictionary<string, string>>(),
            Arg.Any<long>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task RunEventLoopAsync_ShouldReturnTrue_WhenNodeReturnsFailedCompleted(
        [Frozen] ILogger<BpmnEngine> logger,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter)
    {
        // Тест проверяет, что RunEventLoopAsync возвращает true, когда нода возвращает FailedCompleted
        // Arrange
        var engine = Substitute.ForPartsOf<BpmnEngine>(logger, historyNodeStateWriter);

        var context = Substitute.For<IContextBpmnProcess>();
        context.IdBpmnProcess.Returns("TestProcess");
        context.TokenProcess.Returns("TestToken");

        var node = Substitute.For<IBpmnNode>();
        node.Id.Returns("Node1");
        node.ExecuteAsync(
                Arg.Any<ProcessModel>(),
                Arg.Any<IContextBpmnProcess>(),
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ConcurrentDictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BpmnNodeResult
            {
                Status = StatusNode.FailedCompleted,
                Tokens = new List<Token>()
            }));

        var processModel = new ProcessModel();
        processModel.Nodes["Node1"] = node;


        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();
        var eventQueue = new ConcurrentQueue<Token>();
        eventQueue.Enqueue(new Token { CurrentNodeId = "Node1" });

        var cts = CancellationToken.None;

        // Act
        var result = await engine.RunEventLoopAsync(
            context,
            processModel,
            nodeStateRegistry,
            errorRegistry,
            eventQueue,
            cts);

        // Assert
        Assert.True(result);
        Assert.True(nodeStateRegistry.ContainsKey("Node1"));
        Assert.Equal(StatusNode.Works, nodeStateRegistry["Node1"]);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task RunEventLoopAsync_ShouldProcessMultipleNodes(
        [Frozen] ILogger<BpmnEngine> logger,
        [Frozen] IHistoryNodeStateWriter historyNodeStateWriter)
    {
        // Тест проверяет, что RunEventLoopAsync обрабатывает несколько нод последовательно
        // Arrange
        var engine = Substitute.ForPartsOf<BpmnEngine>(logger, historyNodeStateWriter);

        var context = Substitute.For<IContextBpmnProcess>();
        context.IdBpmnProcess.Returns("TestProcess");
        context.TokenProcess.Returns("TestToken");

        var node1 = Substitute.For<IBpmnNode>();
        node1.Id.Returns("Node1");
        node1.ExecuteAsync(
                Arg.Any<ProcessModel>(),
                Arg.Any<IContextBpmnProcess>(),
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ConcurrentDictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BpmnNodeResult
            {
                Status = StatusNode.NormalCompleted,
                Tokens = new List<Token> { new Token { CurrentNodeId = "Node2" } }
            }));

        var node2 = Substitute.For<IBpmnNode>();
        node2.Id.Returns("Node2");
        node2.ExecuteAsync(
                Arg.Any<ProcessModel>(),
                Arg.Any<IContextBpmnProcess>(),
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ConcurrentDictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new BpmnNodeResult
            {
                Status = StatusNode.AllBpmnProcessCompleted,
                Tokens = new List<Token>()
            }));

        var processModel = new ProcessModel();
        processModel.Nodes["Node1"] = node1;
        processModel.Nodes["Node2"] = node2;

        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();
        var eventQueue = new ConcurrentQueue<Token>();
        eventQueue.Enqueue(new Token { CurrentNodeId = "Node1" });

        var cts = CancellationToken.None;

        // Act
        var result = await engine.RunEventLoopAsync(
            context,
            processModel,
            nodeStateRegistry,
            errorRegistry,
            eventQueue,
            cts);

        // Assert
        Assert.True(result);

        await node1.Received(1).ExecuteAsync(
            Arg.Any<ProcessModel>(),
            Arg.Any<IContextBpmnProcess>(),
            Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
            Arg.Any<ConcurrentDictionary<string, string>>(),
            Arg.Any<CancellationToken>());

        await node2.Received(1).ExecuteAsync(
            Arg.Any<ProcessModel>(),
            Arg.Any<IContextBpmnProcess>(),
            Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
            Arg.Any<ConcurrentDictionary<string, string>>(),
            Arg.Any<CancellationToken>());

        Assert.True(nodeStateRegistry.ContainsKey("Node1"));
        Assert.True(nodeStateRegistry.ContainsKey("Node2"));
        Assert.Equal(StatusNode.Works, nodeStateRegistry["Node1"]);
        Assert.Equal(StatusNode.Works, nodeStateRegistry["Node2"]);
    }
}