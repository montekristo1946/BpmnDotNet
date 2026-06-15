using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.BpmnEngineDomain.Activity;

public class ServiceTaskTest
{
    private readonly Fixture _fixture = new Fixture();
    ConcurrentDictionary<string, StatusNode> _nodeStateRegistry = new();

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenContextIsNull(
        [Frozen] ILogger<ServiceTask> logger,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new ServiceTask(logger, handler, currentId);
        IContextBpmnProcess? contextBpmnProcess = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess!, _nodeStateRegistry,[], CancellationToken.None));

        Assert.Equal("context", exception.ParamName);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenActivityHandlerIsNull(
        [Frozen] ILogger<ServiceTask> logger,
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess)
    {
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => null!);

        // Arrange
        var sut = new ServiceTask(logger, handler, _fixture.Create<string>()) { ActivityHandlerAsync = null! };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,[], CancellationToken.None));

        Assert.Equal("ActivityHandlerAsync", exception.ParamName);
    }

    [Fact]
    internal async Task ExecuteAsync_CallActivityHandlerAsync_CountCall()
    {
        int countCall = 0;
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) =>
        {
            countCall++;
            return Task.CompletedTask;
        });
        var sut = new ServiceTask(Substitute.For<ILogger<ServiceTask>>(), handler, _fixture.Create<string>());

        var processModel = _fixture.Create<ProcessModel>();
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,[]);

        Assert.Equal(1, countCall);
    }


    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnCompletedStatusAndTokens_WhenNextNodesExist(
        [Frozen] ILogger<ServiceTask> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow[] nextNodes)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new ServiceTask(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,[], CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.NormalCompletedNode, result.Status);
        Assert.Single(result.Tokens.ToArray());

        foreach (var token in result.Tokens)
        {
            Assert.Contains(nextNodes, n => n.IdResource == token.CurrentNodeId);
        }
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnFailedStatus_WhenActivityHandlerThrowsException(
        [Frozen] ILogger<ServiceTask> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => throw expectedException);
        var sut = new ServiceTask(logger, handler, currentId);
        ConcurrentDictionary<string, string> errorRegistry = new();
        
        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,errorRegistry, CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedCompletedNode, result.Status);
        Assert.Empty(result.Tokens);

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ServiceTask:ExecuteAsync] Exception")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
        Assert.Contains("Test exception", errorRegistry[currentId]);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldCallActivityHandlerWithCorrectParameters(
        [Frozen] ILogger<ServiceTask> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken cancellationToken)
    {
        // Arrange
        var handlerCalled = false;
        IContextBpmnProcess? capturedContext = null;
        CancellationToken capturedToken = default;

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((ctx, ct) =>
        {
            handlerCalled = true;
            capturedContext = ctx;
            capturedToken = ct;
            return Task.CompletedTask;
        });
        // Arrange
        var sut = new ServiceTask(logger, handler, currentId);

        // Act
        await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [],cancellationToken);

        // Assert
        Assert.True(handlerCalled);
        Assert.Same(contextBpmnProcess, capturedContext);
        Assert.Equal(cancellationToken, capturedToken);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldLogWarning_WhenNextNodesNotFound(
        [Frozen] ILogger<ServiceTask> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        // Arrange
        var sut = new ServiceTask(logger, handler, currentId);

        // Act
        var res = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [],CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedCompletedNode, res.Status);
        Assert.Empty(res.Tokens.ToArray());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckFillNodeStateRegistry_NormalCompletedNode(
        [Frozen] ILogger<ServiceTask> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow nextNode)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new ServiceTask(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, [nextNode]);

        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [],CancellationToken.None);

        // Assert
        Assert.Equal(2, _nodeStateRegistry.Count);
        var stateSub = _nodeStateRegistry.TryGetValue(sut.Id, out var statusNodeSub);
        Assert.True(stateSub);
        Assert.Equal(StatusNode.NormalCompletedNode, statusNodeSub);
        var stateFlow = _nodeStateRegistry.TryGetValue(nextNode.IdFlow, out var statusFlow);
        Assert.True(stateFlow);
        Assert.Equal(StatusNode.NormalCompletedNode, statusFlow);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckFillNodeStateRegistry_FailedCompletedNode(
        [Frozen] ILogger<ServiceTask> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow nextNode)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => throw expectedException);
        var sut = new ServiceTask(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, [nextNode]);

        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [],CancellationToken.None);

        // Assert
        Assert.Single(_nodeStateRegistry);
        var stateSub = _nodeStateRegistry.TryGetValue(sut.Id, out var statusNodeSub);
        Assert.True(stateSub);
        Assert.Equal(StatusNode.FailedCompletedNode, statusNodeSub);
    }
}