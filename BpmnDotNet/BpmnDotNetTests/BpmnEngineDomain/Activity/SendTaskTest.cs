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

public class SendTaskTest
{
    private readonly Fixture _fixture = new();
    ConcurrentDictionary<string, StatusNode> _nodeStateRegistry = new();

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenContextIsNull(
        [Frozen] ILogger<SendTask> logger,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new SendTask(logger,handler,currentId);
        IContextBpmnProcess? contextBpmnProcess = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess!, _nodeStateRegistry, CancellationToken.None));

        Assert.Equal("contextBpmnProcess", exception.ParamName);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenActivityHandlerIsNull(
        [Frozen] ILogger<SendTask> logger,
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess)
    {
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => null!);
        
        // Arrange
        var sut = new SendTask(logger,handler,_fixture.Create<string>()) { ActivityHandlerAsync = null! };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, CancellationToken.None));

        Assert.Equal("ActivityHandlerAsync", exception.ParamName);
    }

    [Fact]
    internal async Task ExecuteAsync_CallActivityHandlerAsync_CountCall()
    {
        int countCall = 0;
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) =>  {
            countCall++;
            return Task.CompletedTask;
        });
        var sut = new SendTask(Substitute.For<ILogger<SendTask>>(),handler,_fixture.Create<string>());

        var processModel = _fixture.Create<ProcessModel>();
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry);

        Assert.Equal(1, countCall);
    }


    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnCompletedStatusAndTokens_WhenNextNodesExist(
        [Frozen] ILogger<SendTask> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow[] nextNodes)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new SendTask(logger,handler,currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        // Act
        var result = await sut.ExecuteAsync(processModel, contextBpmnProcess,_nodeStateRegistry,  CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.NormalCompletedNode, result.Status);
        Assert.Equal(nextNodes.Length, result.Tokens.ToArray().Length);

        foreach (var token in result.Tokens)
        {
            Assert.Contains(nextNodes, n => n.IdResource == token.CurrentNodeId);
        }
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnFailedStatus_WhenActivityHandlerThrowsException(
        [Frozen] ILogger<SendTask> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => throw expectedException);
        var sut = new SendTask(logger,handler,currentId) ;

        // Act
        var result = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedCompletedNode, result.Status);
        Assert.Empty(result.Tokens);

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[SendTask:ExecuteAsync] Exception")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldCallActivityHandlerWithCorrectParameters(
        [Frozen] ILogger<SendTask> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken cancellationToken)
    {
        // Arrange
        var handlerCalled = false;
        IContextBpmnProcess? capturedContext = null;
        CancellationToken capturedToken = CancellationToken.None;

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((ctx, ct) =>
        {        handlerCalled = true;
            capturedContext = ctx;
            capturedToken = ct;
            return Task.CompletedTask;
        });
        // Arrange
        var sut = new SendTask(logger,handler,currentId) ;

        // Act
        await sut.ExecuteAsync(processModel, contextBpmnProcess,_nodeStateRegistry,  cancellationToken);

        // Assert
        Assert.True(handlerCalled);
        Assert.Same(contextBpmnProcess, capturedContext);
        Assert.Equal(cancellationToken, capturedToken);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldLogWarning_WhenNextNodesNotFound(
        [Frozen] ILogger<SendTask> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        // Arrange
        var sut = new SendTask(logger,handler,currentId) ;

        // Act
       var res = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedCompletedNode,res.Status);
        Assert.Empty(res.Tokens.ToArray());
    }
}