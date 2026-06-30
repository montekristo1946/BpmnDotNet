using System.Collections.Concurrent;
using AutoFixture;
using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.BpmnEngineDomain.Handlers;
using BpmnDotNet.Handlers;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.BpmnEngineDomain.Handlers;

public class ProcessModelBuilderTest
{
    private readonly IXmlSerializationProcessSection _xmlSerializationProcessSection;
    private readonly ProcessModelBuilder _processModelBuilder;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Fixture _fixture;

    public ProcessModelBuilderTest()
    {
        _xmlSerializationProcessSection = new XmlSerializationProcessSection();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _processModelBuilder = new ProcessModelBuilder(_loggerFactory);
        _fixture = new Fixture();
    }

    [Fact]
    public void Build_FullPass_ProcessModel()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_3.bpmn");
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>
        {
            ["StartEvent_01"] = (ctx, ct) => Task.CompletedTask,
            ["Activity_01"] = (ctx, ct) => Task.CompletedTask,
            ["Event_01"] = (ctx, ct) => Task.CompletedTask,
        };

        var res = _processModelBuilder.Build(diagram, handlers);

        Assert.NotNull(res);
        Assert.Equal(2, res.Flows.Count);
        Assert.Equal(2, res.FlowsBySource.Count);
        Assert.Equal(2, res.FlowsByTarget.Count);
        Assert.Equal(3, res.Nodes.Count);
    }

    [Fact]
    public void BuildFlowsBySourceIndex_FullPass_CountTarget()
    {
        // Arrange
        var flows = new List<Flow>
        {
            new() { Id = string.Empty, SourceId = "source1", TargetId = "target1" },
            new() { Id = string.Empty, SourceId = "source2", TargetId = "target2" },
            new() { Id = string.Empty, SourceId = "source1", TargetId = "target3" }
        };

        // Act
        var result = _processModelBuilder.BuildFlowsBySourceIndex(flows);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2,result["source1"].Length);
        Assert.Single(result["source2"]);
    }

    [Fact]
    public void BuildFlowsByTargetIndex_FullPass_CountTarget()
    {
        // Arrange
        var flows = new List<Flow>
        {
            new() { Id = string.Empty, SourceId = "source1", TargetId = "target1" },
            new() { Id = string.Empty, SourceId = "source2", TargetId = "target2" },
            new() { Id = string.Empty, SourceId = "source1", TargetId = "target3" },
            new() { Id = string.Empty, SourceId = "source4", TargetId = "target2" },
        };

        // Act
        var result = _processModelBuilder.BuildFlowsByTargetIndex(flows);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(2,result["target2"].Length);
        Assert.Single(result["target1"]);
        Assert.Single(result["target3"]);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void CreateGenericActivity_ShouldAddNodeWithFoundHandler_WhenHandlerExists(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen] ILogger<ProcessModelBuilderTestHelperNode> logger,
        string id,
        IContextBpmnProcess context,
        CancellationToken ct)
    {
        // Arrange
        // Настраиваем фабрику логгеров возвращать мок логгера для нашего типа
        loggerFactory.CreateLogger<ProcessModelBuilderTestHelperNode>().Returns(logger);
        
        var sut = new ProcessModelBuilder(loggerFactory); 
        var processModel = new ProcessModel();
        
        var isHandlerCalled = false;
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>();
        handlers.TryAdd(id, (_, _) => { isHandlerCalled = true; return Task.CompletedTask; });

        // Act
        sut.CreateGenericActivity<ProcessModelBuilderTestHelperNode>(id, handlers, processModel);

        // Assert
        Assert.True(processModel.Nodes.TryGetValue(id, out var node));
        var typedNode = Assert.IsType<ProcessModelBuilderTestHelperNode>(node);
        
        Assert.Equal(id, typedNode.Id);
        
        // Проверяем, что в ноду записался именно наш хэндлер
        typedNode.ActivityHandlerAsync(context, ct);
        Assert.True(isHandlerCalled);
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void CreateGenericActivity_ShouldAddNodeWithMoqHandler_WhenHandlerDoesNotExist(
        [Frozen] ILoggerFactory loggerFactory,
        [Frozen]  ILogger<ProcessModelBuilder> nodeLogger,
        string id)
    {
        // Arrange
        loggerFactory.CreateLogger<ProcessModelBuilder>().Returns(nodeLogger);
        
        var sut = new ProcessModelBuilder(loggerFactory);
        var processModel = new ProcessModel();
        
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>();

        // Act
        sut.CreateGenericActivity<ProcessModelBuilderTestHelperNode>(id, handlers, processModel);

        // Assert
        Assert.True(processModel.Nodes.TryGetValue(id, out var node));
        var typedNode = Assert.IsType<ProcessModelBuilderTestHelperNode>(node);
        
        Assert.Equal(id, typedNode.Id);
        Assert.NotNull(typedNode.ActivityHandlerAsync); // Хэндлер проставился (MoqHandler)
        
        // Проверяем, что было залогировано предупреждение/отладка о неизвестном хэндлере
        nodeLogger.Received(1).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ProcessModelBuilder:CreateGenericActivity] Unknown get handlers; Id:")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal void CreateGenericActivity_ShouldUpdateExistingNode_WhenNodeWithSameIdAlreadyExists(
        [Frozen] ILoggerFactory loggerFactory,
        IBpmnNode oldNode,
        string id)
    {
        // Arrange
        var sut = new ProcessModelBuilder(loggerFactory);
        var processModel = new ProcessModel();
        
        // Кладём в модель старую ноду с тем же ID
        processModel.Nodes.TryAdd(id, oldNode);
        
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>();

        // Act
        sut.CreateGenericActivity<ProcessModelBuilderTestHelperNode>(id, handlers, processModel);

        // Assert
        Assert.True(processModel.Nodes.TryGetValue(id, out var currentNode));
        // Проверяем, что старая нода успешно заменилась на новую TestBpmnNode
        Assert.NotSame(oldNode, currentNode);
        Assert.IsType<ProcessModelBuilderTestHelperNode>(currentNode);
    }
}