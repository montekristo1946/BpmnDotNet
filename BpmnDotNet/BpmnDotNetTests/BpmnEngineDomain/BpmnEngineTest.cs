using System.Collections.Concurrent;
using System.Reflection;
using AutoFixture;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.BpmnEngineDomain.Handlers;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.BpmnEngineDomain;

public class BpmnEngineTest
{
    private readonly IXmlSerializationProcessSection _xmlSerializationProcessSection;
    private readonly ProcessModelBuilder _processModelBuilder;
    private readonly ILogger<ProcessModelBuilder> _loggerProcessModelBuilder;
    private readonly BpmnEngine _bpmnEngine;
    private readonly ILogger<BpmnEngine> _logger;
    private readonly Fixture _fixture;

    public BpmnEngineTest()
    {
        _xmlSerializationProcessSection = new XmlSerializationProcessSection();
        _loggerProcessModelBuilder = Substitute.For<ILogger<ProcessModelBuilder>>();
        _processModelBuilder = new ProcessModelBuilder(_loggerProcessModelBuilder);
        _logger =  Substitute.For<ILogger<BpmnEngine>>();
        _bpmnEngine = new BpmnEngine(_logger);
        _fixture = new Fixture();
    }
    [Fact(Skip = "Placeholder test is not implemented")]
    public async Task StartProcessAsync_FullPass_CallMethod()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_3.bpmn");
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>
        {
            ["StartEvent_01"] = (ctx, ct) => Task.CompletedTask,
            ["Activity_01"] = (ctx, ct) => Task.CompletedTask,
            ["Event_01"] = (ctx, ct) => Task.CompletedTask,
        };
        var processModel = _processModelBuilder.Build(diagram, handlers);
        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(500));
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await _bpmnEngine.StartProcessAsync(contextBpmnProcess,processModel,cancellationToken.Token);

        await res.ProcessTask.WaitAsync(cancellationToken.Token);
        
        throw new NotImplementedException();
    }

    [Fact]
    public void CreateStartToken_WithStartServiceTask_EnqueuesStartToken()
    {
        var startTask = new ServiceTask
        {
            Id = "StartEvent_01",
            ActivityHandlerAsync = (context, ct) => Task.CompletedTask,
        };
        var serviceTask = _fixture.Create<ServiceTask>();
        var endEvent = _fixture.Create<EndEvent>();

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

        var eventQueueField = typeof(BpmnEngine).GetField("_eventQueue", BindingFlags.Instance | BindingFlags.NonPublic);
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
}