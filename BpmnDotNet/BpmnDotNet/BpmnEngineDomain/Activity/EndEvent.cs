using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.BpmnEngineDomain.Activity;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// EndEvent Activity.
/// </summary>
internal class EndEvent : IBpmnNode
{
    private readonly ILogger<EndEvent> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndEvent"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    /// <param name="handlerAsync">Func метода клиента.</param>
    /// <param name="id">Id на Bpmn схеме.</param>
    public EndEvent(
        ILogger<EndEvent> logger,
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
        if (contextBpmnProcess == null)
        {
            throw new ArgumentNullException(nameof(contextBpmnProcess));
        }

        if (ActivityHandlerAsync == null)
        {
            throw new ArgumentNullException(nameof(ActivityHandlerAsync));
        }

        StatusNode statusBpmnEngine;
        try
        {
            await ActivityHandlerAsync(contextBpmnProcess, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(Id, out var nextNodes);
            if (isGetNextNodes || nextNodes is not null)
            {
                throw new InvalidDataException($"[ExclusiveGateway:ExecuteAsync] EndEvent must be the final event, IdNode:{Id}");
            }

            statusBpmnEngine = StatusNode.AllBpmnProcessCompleted;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[EndEvent:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedCompletedNode;
        }

        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = [],
        };
    }
}