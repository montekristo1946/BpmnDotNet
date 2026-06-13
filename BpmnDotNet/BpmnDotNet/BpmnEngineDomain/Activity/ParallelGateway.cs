namespace BpmnDotNet.BpmnEngineDomain.Activity;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

/// <summary>
/// ParallelGateway Activity.
/// </summary>
internal class ParallelGateway : IBpmnNode
{
    private readonly ILogger<ParallelGateway> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelGateway"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    /// <param name="handlerAsync">Func метода клиента.</param>
    /// <param name="id">Id на Bpmn схеме.</param>
    public ParallelGateway(
        ILogger<ParallelGateway> logger,
        Func<IContextBpmnProcess, CancellationToken, Task> handlerAsync,
        string id)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ActivityHandlerAsync = handlerAsync ?? throw new ArgumentNullException(nameof(handlerAsync));
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new ArgumentNullException(nameof(Id));
        }
    }

    /// <inheritdoc/>
    public string Id { get; init; }

    /// <inheritdoc/>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; } = null!;

    /// <inheritdoc/>
    public async Task<BpmnNodeResult> ExecuteAsync(
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess,
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processModel);
        ArgumentNullException.ThrowIfNull(contextBpmnProcess);
        ArgumentNullException.ThrowIfNull(ActivityHandlerAsync);
        ArgumentNullException.ThrowIfNull(nodeStateRegistry);

        var statusBpmnEngine = StatusNode.WorksNode;
        nodeStateRegistry[Id] = statusBpmnEngine;

        Token[] nextTokens = [];
        try
        {
            var isFinishInputFlow = CheckCompletedAllInputFlow(nodeStateRegistry, processModel);
            if (!isFinishInputFlow)
            {
                return new BpmnNodeResult()
                {
                    Status = statusBpmnEngine,
                    Tokens = nextTokens,
                };
            }

            await ActivityHandlerAsync(contextBpmnProcess, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(Id, out var nextNodes);

            if (!isGetNextNodes || nextNodes is null || nextNodes.Length == 0)
            {
                throw new InvalidDataException(
                    $"[ParallelGateway:ExecuteAsync] FlowsBySource dictionary returned false, IdNode:{Id}");
            }

            nextTokens = nextNodes.Select(p => new Token
            {
                CurrentNodeId = p.IdResource,
            }).ToArray();

            foreach (var directionFlow in nextNodes)
            {
                nodeStateRegistry[directionFlow.IdFlow] = StatusNode.NormalCompletedNode;
            }

            statusBpmnEngine = StatusNode.NormalCompletedNode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[ParallelGateway:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedCompletedNode;
        }

        nodeStateRegistry[Id] = statusBpmnEngine;
        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextTokens,
        };
    }

    /// <summary>
    /// Проверит состояние веток до текущей ноды.
    /// </summary>
    /// <param name="nodeStateRegistry">История выполнения нод.</param>
    /// <param name="processModel">Структура bpmn.</param>
    /// <returns>Завершены все flow.</returns>
    internal virtual bool CheckCompletedAllInputFlow(
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel)
    {
        var isGetTargetNodes = processModel.FlowsByTarget.TryGetValue(Id, out var targetFlows);

        if (!isGetTargetNodes || targetFlows is null || targetFlows.Length == 0)
        {
            throw new InvalidDataException(
                $"[ServiceTask:CheckInputFlow] FlowsBySource dictionary returned false, IdNode:{Id}");
        }

        foreach (var flow in targetFlows)
        {
            var isCheckFlow = nodeStateRegistry.TryGetValue(flow.IdFlow, out var statusNode);
            if (!isCheckFlow || statusNode != StatusNode.NormalCompletedNode)
            {
                return false;
            }
        }

        return true;
    }
}