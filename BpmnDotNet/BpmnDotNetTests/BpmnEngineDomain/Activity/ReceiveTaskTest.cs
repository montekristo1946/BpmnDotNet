using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

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
            nodeStateRegistry.TryAdd(nextNode.IdFlow, StatusNode.NormalCompletedNode);
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
    internal void AreAllPreviousNodesCompleted_WhenAnyNodeNotCompleted_ReturnsFalse(
        [Frozen] ILogger<ReceiveTask> logger,
        [Frozen] ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        DirectionFlow[] nextNodes,
        string currentId,
        StatusNode notCompletedStatus)
    {
        // Arrange
        processModel.FlowsByTarget.TryAdd(currentId, nextNodes);

        // Первый узел завершен
        if (nextNodes.Length > 0)
        {
            nodeStateRegistry.TryAdd(nextNodes[0].IdFlow, StatusNode.NormalCompletedNode);
        }

        // Остальные узлы имеют незавершенный статус
        for (int i = 1; i < nextNodes.Length; i++)
        {
            nodeStateRegistry.TryAdd(nextNodes[i].IdFlow, notCompletedStatus);
        }

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act
        var result = sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId);

        // Assert
        Assert.False(result);
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
    internal void AreAllPreviousNodesCompleted_WhenSomeNodeStateMissing_ReturnsFalse(
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
            nodeStateRegistry.TryAdd(nextNodes[0].IdFlow, StatusNode.NormalCompletedNode);
        }
        // Остальные узлы отсутствуют в реестре

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ReceiveTask>(logger, handler, currentId);

        // Act
        var result = sut.AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, currentId);

        // Assert
        Assert.False(result);
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
        string currentId,
        object message)
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
        var result = await sut.ExecuteAsync(processModel, context, nodeStateRegistry);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusNode.AllBpmnProcessCompleted, result.Status);
        Assert.NotEmpty(result.Tokens);
        await handler.Received(1).Invoke(context, Arg.Any<CancellationToken>());
    }
}