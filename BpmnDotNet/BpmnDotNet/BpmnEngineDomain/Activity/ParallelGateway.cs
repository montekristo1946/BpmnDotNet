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
    public string Id { get; init; }

    /// <inheritdoc/>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; } = null!;

    /// <inheritdoc/>
    public async Task<BpmnNodeResult> ExecuteAsync(
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();

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
            //TODO: перед выполнением добавить проверить выполнили все входящие ноды.
            
            
            await ActivityHandlerAsync(contextBpmnProcess, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(Id, out var nextNodes);

            if (!isGetNextNodes || nextNodes is null || nextNodes.Length == 0)
            {
                throw new InvalidDataException(
                    $"[ServiceTask:ParallelGateway] FlowsBySource dictionary returned false, IdNode:{Id}");
            }

            nextTokens = nextNodes?.Select(p => new Token
            {
                CurrentNodeId = p.IdResource,
            }).ToArray() ?? [];
            statusBpmnEngine = StatusNode.NormalCompletedNode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[ServiceTask:ParallelGateway] Exception");
            statusBpmnEngine = StatusNode.FailedCompletedNode;
        }

        throw new NotImplementedException();
        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextTokens,
        };
    }
}