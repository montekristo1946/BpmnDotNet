using Microsoft.Extensions.Logging;

namespace BpmnDotNet.BpmnEngineDomain.Activity;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// StartEvent Activity.
/// </summary>
internal class StartEvent : IBpmnNode
{
    private readonly ILogger<StartEvent> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartEvent"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    public StartEvent(ILogger<StartEvent> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; } = null!;

    /// <inheritdoc/>
    public virtual async Task<BpmnNodeResult> ExecuteAsync(
        ProcessModel processModel,
        string currentId,
        IContextBpmnProcess contextBpmnProcess,
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

        var statusBpmnEngine = StatusNode.WorksNode;
        Token[] nextTokens = [];
        try
        {
            await ActivityHandlerAsync(contextBpmnProcess, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(currentId, out var nextNodes);
            if (!isGetNextNodes)
            {
                _logger.LogWarning("[StartEvent:ExecuteAsync] FlowsBySource dictionary returned false IdNode:{IdNode}",
                    currentId);
            }

            nextTokens = nextNodes?.Select(p => new Token
            {
                CurrentNodeId = p.IdResource,
            }).ToArray() ?? [];
            statusBpmnEngine = StatusNode.CompletedNode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[StartEvent:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedNode;
        }

        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextTokens,
        };
    }
}