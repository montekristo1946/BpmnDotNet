namespace BpmnDotNet.BpmnEngineDomain.Activity;

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
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; } = null!;

    /// <inheritdoc/>
    public async Task<BpmnNodeResult> ExecuteAsync(
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        /*if (contextBpmnProcess == null)
        {
            throw new ArgumentNullException(nameof(contextBpmnProcess));
        }

        if (ActivityHandlerAsync == null)
        {
            throw new ArgumentNullException(nameof(ActivityHandlerAsync));
        }

        StatusNode statusBpmnEngine;
        Token[] nextTokens = [];
        try
        {
            await ActivityHandlerAsync(contextBpmnProcess, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(currentId, out var nextNodes);
            if (!isGetNextNodes)
            {
                _logger.LogWarning(
                    "[ExclusiveGateway:ExecuteAsync] FlowsBySource dictionary returned false IdNode:{IdNode}",
                    currentId);
            }

            nextTokens = nextNodes?.Select(p => new Token
            {
                CurrentNodeId = p.IdResource,
            }).ToArray() ?? [];
            statusBpmnEngine = StatusNode.NormalCompletedNode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[ExclusiveGateway:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedCompletedNode;
        }

        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextTokens,
        };*/
    }
}