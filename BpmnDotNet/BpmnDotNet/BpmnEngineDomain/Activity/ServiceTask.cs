namespace BpmnDotNet.BpmnEngineDomain.Activity;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

/// <summary>
/// ServiceTask Activity.
/// </summary>
internal class ServiceTask : IBpmnNode
{
    private readonly ILogger<ServiceTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTask"/> class.
    /// </summary>
    /// <param name="logger"><inheritdoc cref="ILogger"/></param>
    public ServiceTask(ILogger<ServiceTask> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; } = null!;

    /// <inheritdoc/>
    public async Task<BpmnNodeResult> ExecuteAsync(
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

        StatusNode statusBpmnEngine;
        Token[] nextTokens = [];
        try
        {
            await ActivityHandlerAsync(contextBpmnProcess, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(currentId, out var nextNodes);
            if (!isGetNextNodes)
            {
                _logger.LogWarning(
                    "[ServiceTask:ExecuteAsync] FlowsBySource dictionary returned false IdNode:{IdNode}",
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
            _logger.LogError(e, "[ServiceTask:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedCompletedNode;
        }

        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextTokens,
        };
    }
}