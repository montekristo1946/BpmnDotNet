using System.Collections.Concurrent;
using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BpmnDotNetTests.BpmnEngineDomain.Activity;

public class ReceiveTaskTest
{
    private readonly IFixture _fixture = new Fixture();

    [Theory]
    [AutoNSubstituteData]
    internal void AreAllPreviousNodesCompleted_WhenAllNodesCompleted_ReturnsTrue(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        processModel.FlowsByTarget.TryAdd(currentId, nextNodes);

        foreach (var nextNode in nextNodes)
        {
            nodeStateRegistry.TryAdd(nextNode.IdFlow, StatusNode.NormalCompleted);
        }

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act
        var result = sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void AreAllPreviousNodesCompleted_WhenAnyNodeNotCompleted_ReturnsTrue(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        processModel.FlowsByTarget.TryAdd(currentId, nextNodes);

        // Первый узел завершен
        if (nextNodes.Length > 0)
        {
            nodeStateRegistry.TryAdd(nextNodes[0].IdFlow, StatusNode.NormalCompleted);
        }

        // Остальные узлы имеют незавершенный статус
        for (int i = 1; i < nextNodes.Length; i++)
        {
            nodeStateRegistry.TryAdd(nextNodes[i].IdFlow, StatusNode.Works);
        }

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act
        var result = sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void AreAllPreviousNodesCompleted_WhenFlowsByTargetDoesNotContainKey_ThrowsInvalidDataException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() =>
            sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId));

        Assert.Contains("[ReceiveTask:AreAllPreviousNodesCompleted]", exception.Message);
        Assert.Contains(currentId, exception.Message);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void AreAllPreviousNodesCompleted_WhenTargetFlowsIsNull_ThrowsInvalidDataException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        processModel.FlowsByTarget.TryAdd(currentId, null);

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() =>
            sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId));

        Assert.Contains("[ReceiveTask:AreAllPreviousNodesCompleted]", exception.Message);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void AreAllPreviousNodesCompleted_WhenTargetFlowsIsEmptyArray_ThrowsInvalidDataException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        processModel.FlowsByTarget.TryAdd(currentId, Array.Empty<DirectionFlow>());

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() =>
            sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId));

        Assert.Contains("[ReceiveTask:AreAllPreviousNodesCompleted]", exception.Message);
    }


    [Theory]
    [AutoNSubstituteData]
    internal void AreAllPreviousNodesCompleted_WhenSomeNodeStateMissing_ReturnsTrue(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        processModel.FlowsByTarget.TryAdd(currentId, nextNodes);

        // Добавляем только первый узел в реестр
        if (nextNodes.Length > 0)
        {
            nodeStateRegistry.TryAdd(nextNodes[0].IdFlow, StatusNode.NormalCompleted);
        }
        // Остальные узлы отсутствуют в реестре

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act
        var result = sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void CheckForMessage_WhenMessageExists_ReturnsTrue(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, object> receivedMessage,
        IContextBpmnProcess context,
        string idNode,
        object message)
    {
        // Arrange
        context.ReceivedMessage.Returns(receivedMessage);
        receivedMessage.TryAdd(idNode, message);

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, "testId");

        // Act
        var result = sut.CheckForMessage(context, idNode);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void CheckForMessage_WhenMessageDoesNotExist_ReturnsFalse(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, object> receivedMessage,
        IContextBpmnProcess context,
        string idNode)
    {
        // Arrange
        context.ReceivedMessage.Returns(receivedMessage);
        // Не добавляем сообщение в словарь

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, "testId");

        // Act
        var result = sut.CheckForMessage(context, idNode);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void CheckForMessage_WhenReceivedMessageIsNull_ThrowsInvalidOperationException(
        [Frozen] ILogger<ReceiveTask> logger,
        IContextBpmnProcess context,
        string idNode)
    {
        // Arrange
        context.ReceivedMessage.Returns((ConcurrentDictionary<string, object>)null!);
        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, "testId");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => sut.CheckForMessage(context, idNode));

        Assert.Contains("[ReceiveTask:CheckForMessage] Not find ReceivedMessage dictionary", exception.Message);
        Assert.Contains("TestProcessId", exception.Message);
        Assert.Contains("TestToken", exception.Message);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenAllParametersValid_AndAllConditionsMet_ReturnsCompletedResult(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        [Frozen] ConcurrentDictionary<string, object> receivedMessage,
        ProcessModel processModel,
        IContextBpmnProcess context,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка AreAllPreviousNodesCompleted - возвращает true
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(), Arg.Any<string>())
            .Returns(true);

        // Настройка CheckForMessage - возвращает true
        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>()).Returns(true);

        // Настройка FlowsBySource для получения следующих узлов
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        context.ReceivedMessage.Returns(receivedMessage);
        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusNode.AllBpmnProcessCompleted, result.Status);
        Assert.NotEmpty(result.Tokens);
        await handler.Received(1).Invoke(context, Arg.Any<CancellationToken>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenPreviousNodesNotCompleted_ReturnsWorksNodeStatus(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка AreAllPreviousNodesCompleted - возвращает false
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(false);

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusNode.Works, result.Status);
        Assert.Empty(result.Tokens);
        await handler.DidNotReceive().Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenMessageNotFound_ReturnsWorksNodeStatus(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка AreAllPreviousNodesCompleted - возвращает true
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        // Настройка CheckForMessage - возвращает false
        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(false);

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusNode.Works, result.Status);
        Assert.Empty(result.Tokens);
        await handler.DidNotReceive().Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenProcessModelIsNull_ThrowsArgumentNullException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ExecuteAsync(null!, context, nodeStateRegistry,[]));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenContextIsNull_ThrowsArgumentNullException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ExecuteAsync(processModel, null, nodeStateRegistry,[]));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenActivityHandlerIsNull_ThrowsArgumentNullException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        [Frozen] ConcurrentDictionary<string, object> receivedMessage,
        ProcessModel processModel,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        var property = typeof(ReceiveTask)
            .GetProperty(nameof(ReceiveTask.ActivityHandlerAsync));
        property!.SetValue(sut, null);


        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenNodeStateRegistryIsNull_ThrowsArgumentNullException(
        [Frozen] ILogger<ReceiveTask> logger,
        ProcessModel processModel,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ExecuteAsync(processModel, context, null!,[]));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenExceptionOccurs_ReturnsFailedStatus(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка успешных проверок
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(true);

        // Настройка FlowsBySource
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Empty(result.Tokens);
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ReceiveTask:ExecuteAsync] Exception")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenNoNextNodes_ThrowsInvalidDataException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка успешных проверок
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(true);

        // Не добавляем currentId в FlowsBySource

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");
        ConcurrentDictionary<string, string> errorRegistry = new();
        
        // Act & Assert
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,errorRegistry);

        Assert.NotNull(result);
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Empty(result.Tokens);
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ReceiveTask:ExecuteAsync] Exception")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenNextNodesIsNull_ThrowsInvalidDataException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка успешных проверок
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(true);

        // Настройка FlowsBySource - добавляем null значение
        processModel.FlowsBySource.TryAdd(currentId, null!);

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act & Assert
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        Assert.NotNull(result);
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Empty(result.Tokens);
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ReceiveTask:ExecuteAsync] Exception")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenNextNodesIsEmpty_ThrowsInvalidDataException(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка успешных проверок
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(true);

        // Настройка FlowsBySource - добавляем пустой массив
        processModel.FlowsBySource.TryAdd(currentId, Array.Empty<DirectionFlow>());

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act & Assert
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        Assert.NotNull(result);
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Empty(result.Tokens);
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ReceiveTask:ExecuteAsync] Exception")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_UpdatesNodeStateRegistry_OnStartAndCompletion(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка успешных проверок
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(true);

        // Настройка FlowsBySource
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        // Assert
        Assert.True(nodeStateRegistry.ContainsKey(currentId));
        Assert.Equal(StatusNode.AllBpmnProcessCompleted, nodeStateRegistry[currentId]);

        if (nextNodes.Length > 0)
        {
            Assert.True(nodeStateRegistry.ContainsKey(nextNodes[0].IdFlow));
            Assert.Equal(StatusNode.NormalCompleted, nodeStateRegistry[nextNodes[0].IdFlow]);
        }
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WhenExceptionOccurs_UpdatesNodeStateToFailed(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка успешных проверок
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(true);

        // Настройка FlowsBySource
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry,[]);

        // Assert
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Equal(StatusNode.FailedCompleted, nodeStateRegistry[currentId]);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_WithCancellationToken_CancelsOperation(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        IContextBpmnProcess context,
        DirectionFlow[] nextNodes,
        string currentId)
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var handler = Substitute.For<Func<IContextBpmnProcess, CancellationToken, Task>>();
        handler.Invoke(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Настройка успешных проверок
        sut.AreAllPreviousNodesCompleted(
                Arg.Any<ConcurrentDictionary<string, StatusNode>>(),
                Arg.Any<ProcessModel>(),
                Arg.Any<string>())
            .Returns(true);

        sut.CheckForMessage(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>())
            .Returns(true);

        // Настройка FlowsBySource
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        context.IdBpmnProcess.Returns("TestProcessId");
        context.TokenProcess.Returns("TestToken");

        // Act
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry, [],cts.Token);

        // Assert
        Assert.NotNull(result);
        await handler.Received(1).Invoke(context, cts.Token);
    }

}