using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Activity;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.BpmnEngineDomain.Handlers;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.BPMNDiagram.BpmnNatation;
using BpmnDotNet.BpmnEngineDomain.Dto;

internal class ProcessModelBuilder
{
    private readonly ILogger<ProcessModelBuilder> _logger;

    public ProcessModelBuilder(ILogger<ProcessModelBuilder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ProcessModel Build(
        BpmnProcessDto bpmnProcessDto,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers)
    {
        var processModel = new ProcessModel();

        foreach (var element in bpmnProcessDto.ElementsFromBody)
        {
            switch (element)
            {
                case StartEventComponent startEvent:
                    CreateStartEvent(startEvent, handlers, processModel);
                    break;
                case EndEventComponent endEvent:
                    CreateEndEvent(endEvent, handlers, processModel);
                    break;
                case ServiceTaskComponent serviceTask:
                    CreateServiceTask(serviceTask, handlers, processModel);
                    break;
                case SequenceFlowComponent flow:
                    CreateSequenceFlow(flow, handlers, processModel);
                    break;
                default:
                    throw new ArgumentException($"Unknown element type: {element.GetType()}");
            }
        }

        return processModel;
    }

    private void CreateServiceTask(
        ServiceTaskComponent serviceTask,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers,
        ProcessModel processModel)
    {
        var id = serviceTask.IdElement;
        var res = handlers.TryGetValue(id, out var handler);

        if (!res || handler is null)
        {
            _logger.LogWarning(
                "[ProcessModelBuilder:CreateServiceTask] Unknown get handlers; Id: {IdElement}", id);
            handler = MoqHandler;
        }

        var bpmnNode = new ServiceTask()
        {
            Handler = handler,
            Id = id,
        };

        processModel.Nodes.AddOrUpdate(id, _ => bpmnNode, (key, oldMessage) => bpmnNode);
    }

    private void CreateSequenceFlow(
        SequenceFlowComponent flow,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers,
        ProcessModel processModel)
    {
        var id = flow.IdElement;

        var bpmnNode = new Flow()
        {
            Id = id,
            SourceId = flow.SourceId,
            TargetId = flow.TargetId,
        };

        processModel.Flow.Enqueue(bpmnNode);
    }

    private void CreateEndEvent(
        EndEventComponent endEvent,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers,
        ProcessModel processModel)
    {
        var id = endEvent.IdElement;
        var res = handlers.TryGetValue(id, out var handler);

        if (!res || handler is null)
        {
            _logger.LogWarning(
                "[ProcessModelBuilder:CreateEndEvent] Unknown get handlers; Id: {IdElement}", id);
            handler = MoqHandler;
        }

        var bpmnNode = new EndEvent()
        {
            Handler = handler,
            Id = id,
        };

        processModel.Nodes.AddOrUpdate(id, _ => bpmnNode, (key, oldMessage) => bpmnNode);
    }

    private void CreateStartEvent(
        StartEventComponent startEvent,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers,
        ProcessModel processModel)
    {
        var id = startEvent.IdElement;
        var res = handlers.TryGetValue(id, out var handler);

        if (!res || handler is null)
        {
            _logger.LogWarning(
                "[ProcessModelBuilder:CreateStartEvent] Unknown get handlers; Id: {IdElement}", id);
            handler = MoqHandler;
        }

        var bpmnNode = new StartEvent()
        {
            Handler = handler,
            Id = id,
        };

        processModel.Nodes.AddOrUpdate(id, _ => bpmnNode, (key, oldMessage) => bpmnNode);
    }

    private Task MoqHandler(IContextBpmnProcess contextBpmnProcess, CancellationToken ctsToken)
    {
        _logger.LogDebug("[MoqHandler] Calling handler");
        return Task.CompletedTask;
    }
}