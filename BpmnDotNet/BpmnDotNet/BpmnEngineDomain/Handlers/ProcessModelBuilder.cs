using BpmnDotNet.BpmnEngineDomain.Abstractions;

namespace BpmnDotNet.BpmnEngineDomain.Handlers;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.BPMNDiagram.BpmnNatation;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
internal class ProcessModelBuilder : IProcessModelBuilder
{
    private readonly ILogger<ProcessModelBuilder> _logger;


    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessModelBuilder"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    public ProcessModelBuilder(ILogger<ProcessModelBuilder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
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

        var flowsBySource = BuildFlowsBySourceIndex(processModel.Flows.Values);
        flowsBySource.ToList().ForEach(kvp =>
            processModel.FlowsBySource.AddOrUpdate(kvp.Key, _ => kvp.Value, (key, oldMessage) => kvp.Value));

        var flowsByTarget = BuildFlowsByTargetIndex(processModel.Flows.Values);
        flowsByTarget.ToList().ForEach(kvp =>
            processModel.FlowsByTarget.AddOrUpdate(kvp.Key, _ => kvp.Value, (key, oldMessage) => kvp.Value));

        return processModel;
    }


    /// <summary>
    ///  Группируем все потоки по SourceId и преобразуем в словарь массивов TargetId потоков.
    /// </summary>
    /// <param name="flows">Flow.</param>
    /// <returns>Словарь с ветками {SourceId:TargetId[]}.</returns>
    internal Dictionary<string, DirectionFlow[]> BuildFlowsBySourceIndex(IEnumerable<Flow> flows)
    {
        var sourceIndex = flows
            .GroupBy(flow => flow.SourceId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(flow => new DirectionFlow(flow.Id, flow.TargetId)).ToArray());

        return sourceIndex;
    }

    /// <summary>
    ///  Группируем все потоки по TargetId и преобразуем в словарь массивов SourceId потоков.
    /// </summary>
    /// <param name="flows">Flow.</param>
    /// <returns>Словарь с ветками {SourceId:TargetId[]}.</returns>
    internal Dictionary<string, DirectionFlow[]> BuildFlowsByTargetIndex(IEnumerable<Flow> flows)
    {
        var sourceIndex = flows
            .GroupBy(flow => flow.TargetId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(flow => new DirectionFlow(flow.Id, flow.SourceId)).ToArray());

        return sourceIndex;
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

        processModel.Flows.AddOrUpdate(id, _ => bpmnNode, (key, oldMessage) => bpmnNode);
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