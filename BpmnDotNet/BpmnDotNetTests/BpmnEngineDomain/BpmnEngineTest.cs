using System.Collections.Concurrent;
using AutoFixture;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain;
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
    [Fact]
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
        
        throw  new NotImplementedException();
    }
}