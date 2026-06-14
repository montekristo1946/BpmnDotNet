namespace BpmnDotNet.BpmnEngineDomain;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BPMNDiagram.BpmnNatation;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

/// <inheritdoc cref="StartProcessAsync" />
internal class BpmnEngine : IBpmnEngine
{
    private readonly ILogger<BpmnEngine> _logger;
    private readonly SemaphoreSlim _semaphore = new(0, 1);
    private readonly ConcurrentQueue<Token> _eventQueue = new();
    private Task? _threadBackground = null;
    private IContextBpmnProcess? _contextBpmnProcess = null;

    public BpmnEngine(ILogger<BpmnEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<BusinessProcessJobStatusV2> StartProcessAsync(
        IContextBpmnProcess contextBpmnProcess,
        ProcessModel processModel,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(contextBpmnProcess);
        ArgumentNullException.ThrowIfNull(processModel);
        if (ct == CancellationToken.None)
        {
            throw new ArgumentNullException(nameof(ct));
        }

        if (_threadBackground != null)
        {
            throw new InvalidOperationException(
                "[BpmnEngine:StartProcessAsync] The thread background has already been started.");
        }

        _contextBpmnProcess = contextBpmnProcess;
        CreateStartToken(processModel);
        _semaphore.Release();
        _threadBackground = Task.Run(() => ThreadBackground(processModel, contextBpmnProcess, ct), ct);
        var jobStatus = new BusinessProcessJobStatusV2
        {
            IdBpmnProcess = contextBpmnProcess.IdBpmnProcess,
            TokenProcess = contextBpmnProcess.TokenProcess,
            ProcessTask = _threadBackground,
            Process = this,
        };

        return Task.FromResult(jobStatus);
    }

    /// <inheritdoc/>
    public bool AddMessageToQueue(Type messageType, object message)
    {
        ArgumentNullException.ThrowIfNull(_contextBpmnProcess);
        ArgumentNullException.ThrowIfNull(messageType);
        ArgumentNullException.ThrowIfNull(message);
        try
        {
            var idNode = GetIdNodeReceiveMessage(_contextBpmnProcess, messageType);
            AddMessageToQueue(idNode, message, _contextBpmnProcess);

            _eventQueue.Enqueue(new Token()
            {
                CurrentNodeId = idNode,
            });

            _semaphore.Release();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[BpmnEngine:AddMessageToQueue] Exception");
        }

        return false;
    }

    /// <summary>
    /// Найдет StartEventComponent в Nodes, запишет в очередь.
    /// </summary>
    /// <param name="processModel">ProcessModel.</param>
    internal void CreateStartToken(ProcessModel processModel)
    {
        var startEvent = processModel.Nodes
            .FirstOrDefault(p => p.Value is StartEvent)
            .Value as StartEvent;

        if (startEvent == null)
        {
            throw new InvalidOperationException("[BpmnEngine:CreateStartToken] No ServiceTask found.");
        }

        var startToken = new Token() { CurrentNodeId = startEvent.Id };
        _eventQueue.Enqueue(startToken);
    }

    private async Task ThreadBackground(
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken ctsToken)
    {
        _logger.LogDebug(
            "[BpmnEngine:ThreadBackground] Starting business process... {IdBpmnProcess} {TokenProcess}",
            contextBpmnProcess.IdBpmnProcess,
            contextBpmnProcess.TokenProcess);

        ConcurrentDictionary<string, StatusNode> stateRegistry = new();
        ConcurrentDictionary<string, string> errorRegistry = new();

        try
        {
            while (!ctsToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(ctsToken);
                var isCompletedBackground =
                    await RunEventLoopAsync(contextBpmnProcess, processModel, stateRegistry, errorRegistry, _eventQueue,
                        ctsToken);
                if (isCompletedBackground)
                {
                    return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("[BpmnEngine:ThreadBackground] Event loop cancelled");

            // TODO: Присваиваем статус процесса Failed.
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[BpmnEngine:ThreadBackground] Exception");

            // TODO: Присваиваем статус процесса Failed.
            return;
        }
        finally
        {
            // TODO: Записывать состояния всего процесса в базу данных.
        }

        _logger.LogDebug(
            "[BpmnEngine:ThreadBackground] End business... {IdBpmnProcess} {TokenProcess}",
            contextBpmnProcess.IdBpmnProcess,
            contextBpmnProcess.TokenProcess);
    }

    private async Task<bool> RunEventLoopAsync(
        IContextBpmnProcess contextBpmnProcess,
        ProcessModel processModel,
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ConcurrentDictionary<string, string> errorRegistry,
        ConcurrentQueue<Token> eventQueue,
        CancellationToken ctsToken)
    {
        while (eventQueue.Count > 0 && !ctsToken.IsCancellationRequested)
        {
            var resGetToken = eventQueue.TryDequeue(out var token);
            if (!resGetToken || token == null)
            {
                _logger.LogError("[BpmnEngine:RunEventLoopAsync] No events queued");
                return false;
            }

            var currentId = token.CurrentNodeId;
            var node = processModel.Nodes[currentId];

            var nodes = await node.ExecuteAsync(
                processModel,
                contextBpmnProcess,
                nodeStateRegistry,
                errorRegistry,
                ctsToken);
            if (nodes is null)
            {
                throw new InvalidOperationException("[BpmnEngine:RunEventLoopAsync] ExecuteAsync returned null.");
            }

            // TODO: Делать snapshot _nodeStateRegistry в ElasticSearch

            if (nodes.Status == StatusNode.FailedCompletedNode || nodes.Status == StatusNode.AllBpmnProcessCompleted)
            {
                return true;
            }

            nodes.Tokens.ToList().ForEach(eventQueue.Enqueue);
        }

        return false;
    }


    /// <summary>
    /// Добавит в словарь сообщение.
    /// </summary>
    /// <param name="idNode">ID node.</param>
    /// <param name="message">Сообщение.</param>
    /// <param name="context"><inheritdoc cref="IContextBpmnProcess"/></param>
    /// <returns>bool.</returns>
    internal virtual bool AddMessageToQueue(string idNode, object message, IContextBpmnProcess context)
    {
        var messageReceiveTask = context as IMessageReceiveTask;

        var dic = messageReceiveTask?.ReceivedMessage;

        if (dic is null)
        {
            var textMessage =
                $"[BpmnEngine:AddMessageToQueue] Not find ReceivedMessage dictionary" +
                $"IdBpmnProcess {context.IdBpmnProcess} {context.TokenProcess}";
            throw new InvalidOperationException(textMessage);
        }

        dic[idNode] = message;
        return true;
    }

    /// <summary>
    /// Получить из IContextBpmnProcess id node для выполнения.
    /// </summary>
    /// <param name="context"><inheritdoc cref="IContextBpmnProcess"/></param>
    /// <param name="messageType">Тип сообщения.</param>
    /// <returns>Id node.</returns>
    internal virtual string GetIdNodeReceiveMessage(IContextBpmnProcess context, Type messageType)
    {
        var messageReceiveTask = context as IMessageReceiveTask;

        var dic = messageReceiveTask?.RegistrationMessagesType;

        if (dic is null)
        {
            var textMessage =
                $"[BpmnEngine:GetTypeNameMessage] Not find registrationMessagesType dictionary {messageType} " +
                $"IdBpmnProcess {context.IdBpmnProcess} {context.TokenProcess}";
            throw new InvalidOperationException(textMessage);
        }

        var resGet = dic.TryGetValue(messageType, out var idNode);

        if (!resGet || string.IsNullOrWhiteSpace(idNode))
        {
            var textMessage = $"[BpmnEngine:GetTypeNameMessage] Not find registration messages type {messageType} " +
                              $"IdBpmnProcess {context.IdBpmnProcess} {context.TokenProcess}";

            throw new InvalidOperationException(textMessage);
        }

        return idNode;
    }
}