using AutoFixture;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.BpmnEngineDomain.Activity;

public class StartEventTests
{
    private readonly Fixture _fixture = new Fixture();

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenContextIsNull(
        [Frozen] ILogger<StartEvent> logger,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        var sut = new StartEvent(logger) { ActivityHandlerAsync = (_, _) => Task.CompletedTask };
        IContextBpmnProcess? contextBpmnProcess = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, currentId, contextBpmnProcess!, CancellationToken.None));

        Assert.Equal("contextBpmnProcess", exception.ParamName);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenActivityHandlerIsNull(
        [Frozen] ILogger<StartEvent> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var sut = new StartEvent(logger) { ActivityHandlerAsync = null! };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, currentId, contextBpmnProcess, CancellationToken.None));

        Assert.Equal("ActivityHandlerAsync", exception.ParamName);
    }

    [Fact]
    internal async Task ExecuteAsync_CallActivityHandlerAsync_CountCall()
    {
        int countCall = 0;
        var startEvent = new StartEvent(Substitute.For<ILogger<StartEvent>>())
        {
            ActivityHandlerAsync = (_, _) =>
            {
                countCall++;
                return Task.CompletedTask;
            }
        };

        var processModel = _fixture.Create<ProcessModel>();
        var currentId = _fixture.Create<string>();
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await startEvent.ExecuteAsync(processModel, currentId, contextBpmnProcess);

        Assert.Equal(1, countCall);
    }


    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnCompletedStatusAndTokens_WhenNextNodesExist(
        [Frozen] ILogger<StartEvent> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow[] nextNodes)
    {
        // Arrange
        var sut = new StartEvent(logger) { ActivityHandlerAsync = (_, _) => Task.CompletedTask };

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        // Act
        var result = await sut.ExecuteAsync(processModel, currentId, contextBpmnProcess, CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.CompletedNode, result.Status);
        Assert.Equal(nextNodes.Length, result.Tokens.ToArray().Length);

        foreach (var token in result.Tokens)
        {
            Assert.Contains(nextNodes, n => n.IdResource == token.CurrentNodeId);
        }
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnFailedStatus_WhenActivityHandlerThrowsException(
        [Frozen] ILogger<StartEvent> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var sut = new StartEvent(logger)
        {
            ActivityHandlerAsync = (_, _) => throw expectedException
        };

        // Act
        var result = await sut.ExecuteAsync(processModel, currentId, contextBpmnProcess, CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedNode, result.Status);
        Assert.Empty(result.Tokens);

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[StartEvent:ExecuteAsync] Exception")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldCallActivityHandlerWithCorrectParameters(
        [Frozen] ILogger<StartEvent> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken cancellationToken)
    {
        // Arrange
        var handlerCalled = false;
        IContextBpmnProcess? capturedContext = null;
        CancellationToken capturedToken = default;

        var sut = new StartEvent(logger)
        {
            ActivityHandlerAsync = (ctx, ct) =>
            {
                handlerCalled = true;
                capturedContext = ctx;
                capturedToken = ct;
                return Task.CompletedTask;
            }
        };

        // Act
        await sut.ExecuteAsync(processModel, currentId, contextBpmnProcess, cancellationToken);

        // Assert
        Assert.True(handlerCalled);
        Assert.Same(contextBpmnProcess, capturedContext);
        Assert.Equal(cancellationToken, capturedToken);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldLogWarning_WhenNextNodesNotFound(
        [Frozen] ILogger<StartEvent> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var sut = new StartEvent(logger) { ActivityHandlerAsync = (_, _) => Task.CompletedTask };

        // Act
        await sut.ExecuteAsync(processModel, currentId, contextBpmnProcess, CancellationToken.None);

        // Assert
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(currentId)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}