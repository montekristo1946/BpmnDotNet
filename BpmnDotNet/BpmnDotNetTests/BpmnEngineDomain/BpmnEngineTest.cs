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
    private readonly BpmnEngine _bpmnEngine;
    private readonly ILogger<BpmnEngine> _logger;
    private readonly Fixture _fixture;
    private readonly ILoggerFactory _loggerFactory;

    public BpmnEngineTest()
    {
        _xmlSerializationProcessSection = new XmlSerializationProcessSection();
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _processModelBuilder = new ProcessModelBuilder(_loggerFactory);
        _logger = Substitute.For<ILogger<BpmnEngine>>();
        _bpmnEngine = new BpmnEngine(_logger);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task StartProcessAsync_CheckBaseBpmnProcess_CallMethod()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_3.bpmn");
        var count = 0;
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>
        {
            ["StartEvent_01"] = (ctx, ct) =>
            {
                count++;
                return Task.CompletedTask;
            },
            ["Activity_01"] = (ctx, ct) =>
            {
                count++;
                return Task.CompletedTask;
            },
            ["Event_01"] = (ctx, ct) =>
            {
                count++;
                return Task.CompletedTask;
            },
        };
        var processModel = _processModelBuilder.Build(diagram, handlers);
        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();

        var res = await _bpmnEngine.StartProcessAsync(contextBpmnProcess, processModel, cancellationToken.Token);

        await res.ProcessTask.WaitAsync(cancellationToken.Token);

        Assert.Equal(3, count);
    }

    [Fact]
    public void CreateStartToken_WithStartServiceTask_EnqueuesStartToken()
    {
        var logger = Substitute.For<ILogger<StartEvent>>();
        var handler = (Func<IContextBpmnProcess, CancellationToken, Task>)((_, _) => Task.CompletedTask);
        var startTask = new StartEvent(logger, handler, "StartEvent_01");
        var serviceTask = Substitute.For<ServiceTask>(
            Substitute.For<ILogger<ServiceTask>>(),
            handler, _fixture.Create<string>());

        var endEvent = Substitute.For<EndEvent>(
            Substitute.For<ILogger<EndEvent>>(),
            handler, _fixture.Create<string>());

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

        var eventQueueField =
            typeof(BpmnEngine).GetField("_eventQueue", BindingFlags.Instance | BindingFlags.NonPublic);
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

    [Theory]
    [InlineData("StartEvent_1")]
    [InlineData("ServiceTaskFirstHandler")]
    [InlineData("GatewayFirstHandler")]
    [InlineData("SendTaskFirstHandler")]
    [InlineData("ReceiveTaskFirstHandle")]
    [InlineData("Gateway_Second")]
    [InlineData("GatewayThirdHandler")]
    [InlineData("ServiceTaskSecondHandler")]
    [InlineData("ServiceTaskThirdHandler")]
    [InlineData("ServiceTaskFourthHandler")]
    [InlineData("SubProcessFirstHandler")]
    [InlineData("GatewayFourthHandler")]
    [InlineData("End_event")]
    public async Task StartProcessAsync_CheckStartEvent_1_CallMethod(string idActivity)
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_1.bpmn");
        var isCallMethod = false;
        var handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>
        {
            [idActivity] = (ctx, ct) =>
            {
                isCallMethod = true;
                return Task.CompletedTask;
            },
        };
        var processModel = _processModelBuilder.Build(diagram, handlers);
        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(500));
        var contextBpmnProcess = Substitute.For<IContextBpmnProcess>();
        var conditionRoute = new ConcurrentDictionary<string, string>();
        conditionRoute.TryAdd("GatewayFirstHandler","Flow_in_SendTaskFirstHandler");
   

        contextBpmnProcess.ConditionRoute.Returns(conditionRoute);
        var res = await _bpmnEngine.StartProcessAsync(contextBpmnProcess, processModel, cancellationToken.Token);

        await res.ProcessTask.WaitAsync(cancellationToken.Token);

        Assert.True(isCallMethod);
    }
    
  
    

}