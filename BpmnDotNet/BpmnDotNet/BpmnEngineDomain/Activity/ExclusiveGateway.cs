namespace BpmnDotNet.BpmnEngineDomain.Activity;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

/// <summary>
/// ExclusiveGateway Activity.
/// </summary>
internal class ExclusiveGateway : IBpmnNode
{
    private readonly ILogger<ExclusiveGateway> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExclusiveGateway"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    /// <param name="handlerAsync">Func метода клиента.</param>
    /// <param name="id">Id на Bpmn схеме.</param>
    public ExclusiveGateway(
        ILogger<ExclusiveGateway> logger,
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

        Token? nextToken = null;
        try
        {
            await ActivityHandlerAsync(contextBpmnProcess, cancellationToken);

            var isGetCandidateNextNodes = processModel.FlowsBySource.TryGetValue(Id, out var candidateNextNodes);

            if (!isGetCandidateNextNodes || candidateNextNodes is null || candidateNextNodes.Length == 0)
            {
                throw new InvalidDataException(
                    $"[ExclusiveGateway:ExecuteAsync] FlowsBySource dictionary returned false, IdNode:{Id}");
            }

            if (candidateNextNodes.Length == 1)
            {
                var nexFlow = candidateNextNodes.FirstOrDefault() ?? throw new InvalidDataException(
                    "[ExclusiveGateway:ExecuteAsync] Fail get IdResource");

                nextToken = new Token
                {
                    CurrentNodeId = nexFlow.IdResource,
                };
                nodeStateRegistry[nexFlow.IdFlow] = StatusNode.NormalCompletedNode;
            }
            else
            {
                var idRouteFlow = GetRouteFlow(contextBpmnProcess, Id);
                var isGetNextNodes = processModel.Flows.TryGetValue(idRouteFlow, out var flow);
                if (!isGetNextNodes || flow is null)
                {
                    throw new InvalidDataException(
                        $"[ExclusiveGateway:ExecuteAsync] FlowsBySource dictionary returned false, IdNode:{Id}");
                }

                nextToken = new Token
                {
                    CurrentNodeId = flow.TargetId,
                };
                nodeStateRegistry[idRouteFlow] = StatusNode.NormalCompletedNode;
            }

            statusBpmnEngine = StatusNode.NormalCompletedNode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[ExclusiveGateway:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedCompletedNode;
        }

        nodeStateRegistry[Id] = statusBpmnEngine;

        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextToken is null ? Array.Empty<Token>() : new[] { nextToken },
        };
    }

    /// <summary>
    /// Получить id ветки назначения из словаря ConditionRoute.
    /// </summary>
    /// <param name="context"><inheritdoc cref="IContextBpmnProcess"/></param>
    /// <param name="idCurrentNode">id текущей node.</param>
    /// <returns>Id flow которая выбрана.</returns>
    internal virtual string GetRouteFlow(IContextBpmnProcess context, string idCurrentNode)
    {
        var dict = context.ConditionRoute;
        if (dict is null)
        {
            throw new InvalidDataException(
                $" [ExclusiveGateway:GetRouteFlow] Context ConditionRoute dictionary is null");
        }

        if (!dict.TryGetValue(idCurrentNode, out var conditionName) || string.IsNullOrWhiteSpace(conditionName))
        {
            throw new InvalidDataException($" [ExclusiveGateway:GetRouteFlow] " +
                                           $"Couldn't find the condition from gateway:{idCurrentNode}");
        }

        return conditionName;
    }
}