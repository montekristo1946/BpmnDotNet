namespace BpmnDotNet.BpmnEngineDomain.Activity;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

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
        IContextBpmnProcess context,
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ConcurrentDictionary<string, string> errorRegistry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(processModel);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(ActivityHandlerAsync);
        ArgumentNullException.ThrowIfNull(nodeStateRegistry);
        ArgumentNullException.ThrowIfNull(errorRegistry);

        var statusBpmnEngine = StatusNode.Works;
        nodeStateRegistry[Id] = statusBpmnEngine;

        try
        {
            await ActivityHandlerAsync(context, cancellationToken);
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
            statusBpmnEngine = StatusNode.FailedCompleted;
            errorRegistry[Id] = e.Message;
        }

        nodeStateRegistry[Id] = statusBpmnEngine;
        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = [],
        };
    }
}