namespace BpmnDotNet.BpmnEngineDomain.Activity;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

/// <summary>
/// SubProcess Activity.
/// </summary>
internal class SubProcess : IBpmnNode
{
    private readonly ILogger<SubProcess> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubProcess"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    /// <param name="handlerAsync">Func метода клиента.</param>
    /// <param name="id">Id на Bpmn схеме.</param>
    public SubProcess(
        ILogger<SubProcess> logger,
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
        IContextBpmnProcess context,
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processModel);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(ActivityHandlerAsync);
        ArgumentNullException.ThrowIfNull(nodeStateRegistry);

        var statusBpmnEngine = StatusNode.WorksNode;
        nodeStateRegistry[Id] = statusBpmnEngine;

        Token? nextToken = null;
        try
        {
            await ActivityHandlerAsync(context, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(Id, out var nextNodes);

            if (!isGetNextNodes || nextNodes is null || nextNodes.Length == 0)
            {
                throw new InvalidDataException(
                    $"[SubProcess:ExecuteAsync] FlowsBySource dictionary returned false, IdNode:{Id}");
            }

            var nexFlow = nextNodes.FirstOrDefault();
            if (nexFlow is not null)
            {
                nextToken = new Token
                {
                    CurrentNodeId = nexFlow.IdResource,
                };

                nodeStateRegistry[nexFlow.IdFlow] = StatusNode.NormalCompletedNode;
            }

            statusBpmnEngine = StatusNode.NormalCompletedNode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[SubProcess:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedCompletedNode;
        }

        nodeStateRegistry[Id] = statusBpmnEngine;
        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextToken is null ? Array.Empty<Token>() : new[] { nextToken },
        };
    }
}