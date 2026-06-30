namespace BpmnDotNet.BpmnEngineDomain.Activity;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

/// <summary>
/// ReceiveTask Activity.
/// </summary>
internal class ReceiveTask : IBpmnNode
{
    private readonly ILogger<ReceiveTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceiveTask"/> class.
    /// </summary>
    /// <param name="logger">ILogger.</param>
    /// <param name="handlerAsync">Func метода клиента.</param>
    /// <param name="id">Id на Bpmn схеме.</param>
    public ReceiveTask(
        ILogger<ReceiveTask> logger,
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
        Token? nextToken = null;
        nodeStateRegistry[Id] = statusBpmnEngine;

        try
        {
            var isAreAllPreviousNodesCompleted = AreAllPreviousNodesCompleted(nodeStateRegistry, processModel, Id);
            if (!isAreAllPreviousNodesCompleted)
            {
                _logger.LogDebug(
                    "[ReceiveTask:ExecuteAsync] Not  all previous nodes completed, IdNode:{Id}; {IdBpmnProcess}:{TokenProcess}",
                    Id,
                    context.IdBpmnProcess,
                    context.TokenProcess);

                return new BpmnNodeResult()
                {
                    Status = statusBpmnEngine,
                    Tokens = [],
                };
            }

            var isCheckMessage = CheckForMessage(context, Id);
            if (!isCheckMessage)
            {
                _logger.LogDebug(
                    "[ReceiveTask:ExecuteAsync] Message not found, IdNode:{Id}; {IdBpmnProcess}:{TokenProcess}",
                    Id,
                    context.IdBpmnProcess,
                    context.TokenProcess);

                return new BpmnNodeResult()
                {
                    Status = statusBpmnEngine,
                    Tokens = [],
                };
            }

            await ActivityHandlerAsync(context, cancellationToken);
            var isGetNextNodes = processModel.FlowsBySource.TryGetValue(Id, out var nextNodes);
            if (!isGetNextNodes || nextNodes is null || nextNodes.Length == 0)
            {
                throw new InvalidDataException(
                    $"[ReceiveTask:ExecuteAsync] EndEvent must be the final event, IdNode:{Id}");
            }

            var nexFlow = nextNodes.FirstOrDefault();
            if (nexFlow is not null)
            {
                nextToken = new Token
                {
                    CurrentNodeId = nexFlow.IdResource,
                };

                nodeStateRegistry[nexFlow.IdFlow] = StatusNode.NormalCompleted;
            }

            statusBpmnEngine = StatusNode.NormalCompleted;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[ReceiveTask:ExecuteAsync] Exception");
            statusBpmnEngine = StatusNode.FailedCompleted;
            errorRegistry[Id] = e.Message;
        }

        nodeStateRegistry[Id] = statusBpmnEngine;
        return new BpmnNodeResult()
        {
            Status = statusBpmnEngine,
            Tokens = nextToken is null ? Array.Empty<Token>() : new[] { nextToken },
        };
    }

    /// <summary>
    /// Проверяет наличие сообщения в контексте.
    /// </summary>
    /// <param name="context"><inheritdoc cref="IContextBpmnProcess"/></param>
    /// <param name="idNode">Id node.</param>
    /// <returns>Наличие сообщения.</returns>
    internal virtual bool CheckForMessage(IContextBpmnProcess context, string idNode)
    {
        var messageReceiveTask = context as IMessageReceiveTask;

        var dic = messageReceiveTask?.ReceivedMessage;

        if (dic is null)
        {
            var textMessage =
                $"[ReceiveTask:CheckForMessage] Not find ReceivedMessage dictionary" +
                $"IdBpmnProcess {context.IdBpmnProcess} {context.TokenProcess}";
            throw new InvalidOperationException(textMessage);
        }

        var isGetMessage = dic.TryGetValue(idNode, out var message);

        return isGetMessage && message is not null;
    }

    /// <summary>
    /// Проверит завершены ли все входящие ноды.
    /// </summary>
    /// <param name="nodeStateRegistry">Регистр завершеных нод.</param>
    /// <param name="processModel"><inheritdoc cref="ProcessModel"/></param>
    /// <param name="id">ID текущей ноды.</param>
    /// <returns>Результат выполнения.</returns>
    internal virtual bool AreAllPreviousNodesCompleted(
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ProcessModel processModel,
        string id)
    {
        var isGetTargetNodes = processModel.FlowsByTarget.TryGetValue(id, out var targetFlows);

        if (!isGetTargetNodes || targetFlows is null || targetFlows.Length == 0)
        {
            throw new InvalidDataException(
                $"[ReceiveTask:AreAllPreviousNodesCompleted] FlowsBySource dictionary returned false, IdNode:{Id}");
        }

        if (targetFlows.Length > 1)
        {
            _logger.LogWarning(
                "[ReceiveTask:AreAllPreviousNodesCompleted] many input nodes, there should be one. {id}",
                id);
        }

        foreach (var targetFlow in targetFlows)
        {
            var isFindSate = nodeStateRegistry.TryGetValue(targetFlow.IdFlow, out var sate);
            if (isFindSate && sate == StatusNode.NormalCompleted)
            {
                return true;
            }
        }

        return false;
    }
}