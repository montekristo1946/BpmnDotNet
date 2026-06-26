using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.BpmnEngineDomain.Activity;

public class ExclusiveGatewayTest
{
    private readonly IFixture _fixture;
    private readonly ConcurrentDictionary<string, StatusNode> _nodeStateRegistry = new();

    public ExclusiveGatewayTest()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Theory]
    [InlineData("gateway_1", "route_flow_1")]
    [InlineData("gateway_2", "route_flow_2")]
    [InlineData("node_start", "flow_main")]
    public void GetRouteFlow_WhenConditionExists_ReturnsConditionName(string idCurrentNode, string expectedConditionName)
    {
        // Arrange
        var context = Substitute.For<IContextBpmnProcess>();
        var conditionRoute = new ConcurrentDictionary<string, string>();
        conditionRoute.TryAdd(idCurrentNode, expectedConditionName);
        context.ConditionRoute.Returns(conditionRoute);

        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        // Arrange
        var gateway = new ExclusiveGateway(Substitute.For<ILogger<ExclusiveGateway>>(), handler, _fixture.Create<string>());

        // Act
        var result = gateway.GetRouteFlow(context, idCurrentNode);

        // Assert
        Assert.Equal(expectedConditionName, result);
    }
    [Fact]
    public void GetRouteFlow_WhenDictionaryIsNull_ThrowsInvalidDataException()
    {
        // Arrange
        var context = Substitute.For<IContextBpmnProcess>();
        context.ConditionRoute.Returns((ConcurrentDictionary<string, string>)null!);
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var gateway = new ExclusiveGateway(Substitute.For<ILogger<ExclusiveGateway>>(), handler, _fixture.Create<string>());

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            gateway.GetRouteFlow(context, _fixture.Create<string>()));
        Assert.Contains("ConditionRoute dictionary is null", exception.Message);
    }
    
    [Theory]
    [InlineData("gateway_1", "")]
    [InlineData("gateway_2", null)]
    [InlineData("gateway_3", "   ")]
    public void GetRouteFlow_WhenConditionNameIsNullOrWhitespace_ThrowsInvalidDataException(
        string idCurrentNode, string? conditionName)
    {
        // Arrange
        var context = Substitute.For<IContextBpmnProcess>();
        
        var conditionRoute = new ConcurrentDictionary<string, string>();
        conditionRoute.TryAdd(idCurrentNode, conditionName!);
        context.ConditionRoute.Returns(conditionRoute);
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var gateway = new ExclusiveGateway(Substitute.For<ILogger<ExclusiveGateway>>(), handler, _fixture.Create<string>());

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            gateway.GetRouteFlow(context, idCurrentNode));
        Assert.Contains(idCurrentNode, exception.Message);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenContextIsNull(
        [Frozen] ILogger<ExclusiveGateway> logger,
        ProcessModel processModel,
        string currentId)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = new ExclusiveGateway(logger, handler, currentId);
        IContextBpmnProcess? contextBpmnProcess = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess!, _nodeStateRegistry, [], CancellationToken.None));

        Assert.Equal("context", exception.ParamName);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenActivityHandlerIsNull(
        [Frozen] ILogger<ExclusiveGateway> logger,
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess)
    {
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => null!);

        // Arrange
        var sut = new ExclusiveGateway(logger, handler, _fixture.Create<string>()) { ActivityHandlerAsync = null! };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [], CancellationToken.None));

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
        var sub = new ExclusiveGateway(Substitute.For<ILogger<ExclusiveGateway>>(), handler, _fixture.Create<string>());

        var processModel = _fixture.Create<ProcessModel>();
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await sub.ExecuteAsync(processModel, contextBpmnProcess,_nodeStateRegistry,[],CancellationToken.None );

        Assert.Equal(1, countCall);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckMultiFlows_WhenNextNodesExist(
        [Frozen] ILogger<ExclusiveGateway> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        Flow nextFlow,
        DirectionFlow[] nextNodes)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ExclusiveGateway>(logger, handler, currentId);
        sut.GetRouteFlow(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>()).Returns(currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.Flows.TryAdd(currentId, nextFlow);
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);

        // Act
        var result = await sut.ExecuteAsync(processModel, contextBpmnProcess,_nodeStateRegistry,  [], CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.NormalCompleted, result.Status);
        Assert.Single(result.Tokens.ToArray());

        foreach (var token in result.Tokens)
        {
            Assert.Contains(token.CurrentNodeId, nextFlow.TargetId);
        }
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckSingFlows_WhenNextNodesExist(
        [Frozen] ILogger<ExclusiveGateway> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow nextNode)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ExclusiveGateway>(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, [nextNode]);

        // Act
        var result = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [], CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.NormalCompleted, result.Status);
        Assert.Single(result.Tokens.ToArray());

        foreach (var token in result.Tokens)
        {
            Assert.Contains(token.CurrentNodeId, nextNode.IdResource);
        }
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_ShouldReturnFailedStatus_WhenActivityHandlerThrowsException(
        [Frozen] ILogger<ExclusiveGateway> logger,
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => throw expectedException);
        var sut = new ExclusiveGateway(logger,handler,currentId) ;
        ConcurrentDictionary<string, string> errorRegistry = new();
        
        // Act
        var result = await sut.ExecuteAsync(processModel, contextBpmnProcess,_nodeStateRegistry,errorRegistry,   CancellationToken.None);

        // Assert
        Assert.Equal(StatusNode.FailedCompleted, result.Status);
        Assert.Empty(result.Tokens);

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ExclusiveGateway:ExecuteAsync] Exception")),
            expectedException,
            Arg.Any<Func<object, Exception?, string>>());
        
        Assert.Contains("Test exception", errorRegistry[currentId]);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckFillNodeStateRegistryCheckSingFlows_WhenNextNodesExist(
        [Frozen] ILogger<ExclusiveGateway> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow nextNode)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var sut = Substitute.ForPartsOf<ExclusiveGateway>(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, [nextNode]);

        // Act
        var result = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [], CancellationToken.None);

        // Assert
        Assert.Equal(2, _nodeStateRegistry.Count);
        var stateSub = _nodeStateRegistry.TryGetValue(sut.Id, out var statusNodeSub);
        Assert.True(stateSub);
        Assert.Equal(StatusNode.NormalCompleted,statusNodeSub);
        var stateFlow = _nodeStateRegistry.TryGetValue(nextNode.IdFlow, out var statusFlow);
        Assert.True(stateFlow);
        Assert.Equal(StatusNode.NormalCompleted,statusFlow);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckFillNodeStateRegistryCheckMultiFlows_WhenNextNodesExist(
        [Frozen] ILogger<ExclusiveGateway> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow[] nextNodes,
        Flow nextFlow)
    {
        // Arrange
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        
        var sut = Substitute.ForPartsOf<ExclusiveGateway>(logger, handler, currentId);
        sut.GetRouteFlow(Arg.Any<IContextBpmnProcess>(), Arg.Any<string>()).Returns(nextFlow.Id);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, nextNodes);
        processModel.Flows.TryAdd(nextFlow.Id, nextFlow);
        
        // Act
        var result = await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [], CancellationToken.None);

        // Assert
        Assert.Equal(2, _nodeStateRegistry.Count);
        var stateSub = _nodeStateRegistry.TryGetValue(sut.Id, out var statusNodeSub);
        Assert.True(stateSub);
        Assert.Equal(StatusNode.NormalCompleted,statusNodeSub);
        var stateFlow = _nodeStateRegistry.TryGetValue(nextFlow.Id, out var statusFlow);
        Assert.True(stateFlow);
        Assert.Equal(StatusNode.NormalCompleted,statusFlow);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task ExecuteAsync_CheckFillNodeStateRegistry_FailedCompletedNode(
        [Frozen] ILogger<ExclusiveGateway> logger,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
        DirectionFlow nextNode)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => throw expectedException);
        var sut = new ExclusiveGateway(logger, handler, currentId);

        var processModel = _fixture.Create<ProcessModel>();
        processModel.FlowsBySource.TryAdd(currentId, [nextNode]);

        // Act
        var result =
            await sut.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, [], CancellationToken.None);

        // Assert
        Assert.Single(_nodeStateRegistry);
        var stateSub = _nodeStateRegistry.TryGetValue(sut.Id, out var statusNodeSub);
        Assert.True(stateSub);
        Assert.Equal(StatusNode.FailedCompleted,statusNodeSub);
    }
    
}