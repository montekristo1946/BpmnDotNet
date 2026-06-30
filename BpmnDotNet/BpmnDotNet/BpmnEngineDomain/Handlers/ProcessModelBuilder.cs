namespace BpmnDotNet.BpmnEngineDomain.Handlers;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BPMNDiagram.BpmnNatation;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.Dto;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
internal class ProcessModelBuilder : IProcessModelBuilder
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ProcessModelBuilder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessModelBuilder"/> class.
    /// </summary>
    /// <param name="loggerFactory"><inheritdoc cref="ILoggerFactory"/></param>
    public ProcessModelBuilder(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = _loggerFactory.CreateLogger<ProcessModelBuilder>();
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
                case SequenceFlowComponent flow:
                    CreateSequenceFlow(flow, handlers, processModel);
                    break;
                case StartEventComponent component:
                    CreateGenericActivity<StartEvent>(component.IdElement, handlers, processModel);
                    break;
                case EndEventComponent component:
                    CreateGenericActivity<EndEvent>(component.IdElement, handlers, processModel);
                    break;
                case ExclusiveGatewayComponent component:
                    CreateGenericActivity<ExclusiveGateway>(component.IdElement, handlers, processModel);
                    break;
                case ParallelGatewayComponent component:
                    CreateGenericActivity<ParallelGateway>(component.IdElement, handlers, processModel);
                    break;
                case ReceiveTaskComponent component:
                    CreateGenericActivity<ReceiveTask>(component.IdElement, handlers, processModel);
                    break;
                case SendTaskComponent component:
                    CreateGenericActivity<SendTask>(component.IdElement, handlers, processModel);
                    break;
                case ServiceTaskComponent component:
                    CreateGenericActivity<ServiceTask>(component.IdElement, handlers, processModel);
                    break;
                case SubProcessComponent component:
                    CreateGenericActivity<SubProcess>(component.IdElement, handlers, processModel);
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

    /// <summary>
    ///  Динамически создаст Activity.
    /// </summary>
    /// <param name="id">Id элемента.</param>
    /// <param name="handlers">Func handlers.</param>
    /// <param name="processModel"><see cref="ProcessModel"/>.</param>
    /// <typeparam name="TDestination"><see cref="IBpmnNode"/>.</typeparam>
    internal void CreateGenericActivity<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDestination>(
        string id,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers,
        ProcessModel processModel)
        where TDestination : IBpmnNode
    {
        var res = handlers.TryGetValue(id, out var handler);

        if (!res || handler is null)
        {
            _logger.LogDebug(
                "[ProcessModelBuilder:CreateGenericActivity] Unknown get handlers; Id: {IdElement}", id);
            handler = MoqHandler;
        }

        var logger = _loggerFactory.CreateLogger<TDestination>();

        var bpmnNode = (IBpmnNode)Activator.CreateInstance(typeof(TDestination), logger, handler, id)!;

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

    private Task MoqHandler(IContextBpmnProcess contextBpmnProcess, CancellationToken ctsToken)
    {
        _logger.LogDebug("[MoqHandler] Calling handler");
        return Task.CompletedTask;
    }
}