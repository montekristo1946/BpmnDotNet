using System.Collections.Concurrent;
using AutoFixture;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.BpmnEngineDomain.Handlers;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.BpmnEngineDomain.Handlers;

public class ProcessModelBuilderTest
{
    private readonly IXmlSerializationProcessSection _xmlSerializationProcessSection;
    private readonly ProcessModelBuilder _processModelBuilder;
    private readonly ILogger<ProcessModelBuilder> _logger;
    private readonly Fixture _fixture;

    public ProcessModelBuilderTest()
    {
        _xmlSerializationProcessSection = new XmlSerializationProcessSection();
        _logger = Substitute.For<ILogger<ProcessModelBuilder>>();
        _processModelBuilder = new ProcessModelBuilder(_logger);
        _fixture =  new Fixture();
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

        var res =_processModelBuilder.Build(diagram, handlers);
        
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
    }
}