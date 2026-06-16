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

public class EndEventTest
{
    private readonly Fixture _fixture = new();
    ConcurrentDictionary<string, StatusNode> _nodeStateRegistry = new();

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenContextIsNull(
        [Frozen] ILogger<EndEvent> logger,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new EndEvent(logger, handler, currentId);
        IContextBpmnProcess? contextBpmnProcess = null;


        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess!, _nodeStateRegistry,[], CancellationToken.None));

        Assert.Equal("context", exception.ParamName);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenActivityHandlerIsNull(
        [Frozen] ILogger<EndEvent> logger,
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess)
    {
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => null!);

        // Arrange
        var sut = new EndEvent(logger, handler, _fixture.Create<string>()) { ActivityHandlerAsync = null! };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,[],  CancellationToken.None));

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
        var sub = new EndEvent(Substitute.For<ILogger<EndEvent>>(), handler, _fixture.Create<string>());

        var processModel = _fixture.Create<ProcessModel>();
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await sub.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,[] );

        Assert.Equal(1, countCall);
    }


    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnCompletedStatusAndTokens_WhenNextNodesExist(
        [Frozen] ILogger<EndEvent> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow[] nextNodes)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new EndEvent(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [], CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Empty(result.Tokens.ToArray());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnFailedStatus_WhenActivityHandlerThrowsException(
        [Frozen] ILogger<EndEvent> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => throw expectedException);
        var sut = new EndEvent(logger, handler, currentId);

        ConcurrentDictionary<string, string> errorRegistry = new();
        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,errorRegistry, CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Empty(result.Tokens);

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[EndEvent:ExecuteAsync] Exception")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
        
        Assert.Contains("Test exception", errorRegistry[currentId]);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldCallActivityHandlerWithCorrectParameters(
        [Frozen] ILogger<EndEvent> logger,
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
        var sut = new EndEvent(logger, handler, currentId);

        // Act
        await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [], cancellationToken);

        // Assert
        Assert.True(handlerCalled);
        Assert.Same(contextBpmnProcess, capturedContext);
        Assert.Equal(cancellationToken, capturedToken);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldLogWarning_WhenNextNodesNotFound(
        [Frozen] ILogger<EndEvent> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        // Arrange
        var sut = new EndEvent(logger, handler, currentId);

        // Act
        var res = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [],CancellationToken.None);

        // Assert
        Assert.Empty(res.Tokens.ToArray());
        Assert.Equal(StatusNode.AllBpmnProcessCompleted, res.Status);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckFillNodeStateRegistry_NormalCompletedNode(
        [Frozen] ILogger<EndEvent> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new EndEvent(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();

        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry,[], CancellationToken.None);

        // Assert
        Assert.Single(_nodeStateRegistry);
        var stateSub = _nodeStateRegistry.TryGetValue(sut.Id, out var statusNodeSub);
        Assert.True(stateSub);
        Assert.Equal(StatusNode.AllBpmnProcessCompleted, statusNodeSub);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckFillNodeStateRegistry_FailedCompletedNode(
        [Frozen] ILogger<EndEvent> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => throw expectedException);
        var sut = new EndEvent(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();

        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [],CancellationToken.None);

        // Assert
        Assert.Single(_nodeStateRegistry);
        var stateSub = _nodeStateRegistry.TryGetValue(sut.Id, out var statusNodeSub);
        Assert.True(stateSub);
        Assert.Equal(StatusNode.FailedCompleted, statusNodeSub);
    }
}