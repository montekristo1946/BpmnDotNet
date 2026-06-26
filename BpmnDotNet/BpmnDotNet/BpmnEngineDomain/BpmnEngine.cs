namespace BpmnDotNet.BpmnEngineDomain;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Activity;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.HistoryDomain.Abstractions;
using Microsoft.Extensions.Logging;

/// <inheritdoc cref="StartProcessAsync" />
internal class BpmnEngine : IBpmnEngine
{
    private readonly ILogger<BpmnEngine> _logger;
    private readonly SemaphoreSlim _semaphore = new(0, 1);
    private readonly ConcurrentQueue<Token> _eventQueue = new();
    private readonly IHistoryNodeStateWriter _historyNodeStateWriter;
    private Task? _threadBackground = null;
    private IContextBpmnProcess? _contextBpmnProcess = null;
    private long _timeInitInstanse = -1;
    private CancellationTokenSource? _ctsBpmnEngine;
    private int _disposed = 0;
    private int _isProcessCancel = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="BpmnEngine"/> class.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/>.</param>
    /// <param name="historyNodeStateWriter"><see cref="IHistoryNodeStateWriter"/>.</param>
    public BpmnEngine(ILogger<BpmnEngine> logger, IHistoryNodeStateWriter historyNodeStateWriter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _historyNodeStateWriter =
            historyNodeStateWriter ?? throw new ArgumentNullException(nameof(historyNodeStateWriter));
    }

    /// <inheritdoc/>
    public bool IsProcessCancel
    {
        get => Interlocked.CompareExchange(ref _isProcessCancel, 0, 0) == 1;
        private set => Interlocked.Exchange(ref _isProcessCancel, value ? 1 : 0);
    }

    /// <inheritdoc/>
    public async Task<BusinessProcessJobStatus> StartProcessAsync(
        IContextBpmnProcess contextBpmnProcess,
        ProcessModel processModel,
        CancellationToken externalCt)
    {
        ArgumentNullException.ThrowIfNull(contextBpmnProcess);
        ArgumentNullException.ThrowIfNull(processModel);
        if (externalCt == CancellationToken.None)
        {
            throw new ArgumentNullException(nameof(externalCt));
        }

        if (_threadBackground != null)
        {
            throw new InvalidOperationException(
                "[BpmnEngine:StartProcessAsync] The thread background has already been started.");
        }

        _ctsBpmnEngine = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        _contextBpmnProcess = contextBpmnProcess;
        CreateStartToken(processModel);
        _semaphore.Release();
        _timeInitInstanse = DateTime.Now.Ticks;
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _threadBackground = Task.Run(
            () => ThreadBackground(processModel, contextBpmnProcess, startSignal, _ctsBpmnEngine.Token),
            _ctsBpmnEngine.Token);

        await startSignal.Task;
        var jobStatus = new BusinessProcessJobStatus
        {
            IdBpmnProcess = contextBpmnProcess.IdBpmnProcess,
            TokenProcess = contextBpmnProcess.TokenProcess,
            ProcessTask = _threadBackground,
            Process = this,
        };

        return jobStatus;
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

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        try
        {
            if (_ctsBpmnEngine != null)
            {
                await _ctsBpmnEngine.CancelAsync();
            }

            _semaphore.Release();

            if (_threadBackground != null)
            {
                try
                {
                    await _threadBackground.WaitAsync(TimeSpan.FromSeconds(5));
                }
                catch (AggregateException ex) when (
                    ex.InnerExceptions.All(e => e is OperationCanceledException))
                {
                    _logger.LogWarning("[BpmnEngine:Dispose] OperationCanceledException");
                }
            }
        }
        finally
        {
            _ctsBpmnEngine?.Dispose();
            _semaphore?.Dispose();
        }
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

    /// <summary>
    /// Фоновый процесс.
    /// </summary>
    /// <param name="processModel"><see cref="ProcessModel"/>.</param>
    /// <param name="contextBpmnProcess"><see cref="IContextBpmnProcess"/>.</param>
    /// <param name="startSignal"><see cref="TaskCompletionSource"/>.</param>
    /// <param name="ctsToken"><see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal async Task ThreadBackground(
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess,
        TaskCompletionSource startSignal,
        CancellationToken ctsToken)
    {
        _logger.LogDebug(
            "[BpmnEngine:ThreadBackground] Starting business process... {IdBpmnProcess} {TokenProcess}",
            contextBpmnProcess.IdBpmnProcess,
            contextBpmnProcess.TokenProcess);

        // сообщаем вызывающему потоку, что мы запустились
        startSignal.TrySetResult();

        ConcurrentDictionary<string, StatusNode> stateRegistry = new();
        ConcurrentDictionary<string, string> errorRegistry = new();

        try
        {
            while (!ctsToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(ctsToken);
                var isCompletedBackground =
                    await RunEventLoopAsync(
                        contextBpmnProcess,
                        processModel,
                        stateRegistry,
                        errorRegistry,
                        _eventQueue,
                        ctsToken);

                if (isCompletedBackground)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("[BpmnEngine:ThreadBackground] Event loop cancelled");

            var idProcess = $"{contextBpmnProcess.IdBpmnProcess}_{contextBpmnProcess.TokenProcess}";
            stateRegistry[idProcess] = StatusNode.FailedCompleted;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[BpmnEngine:ThreadBackground] Exception");
            var idProcess = $"{contextBpmnProcess.IdBpmnProcess}_{contextBpmnProcess.TokenProcess}";
            stateRegistry[idProcess] = StatusNode.FailedCompleted;
            errorRegistry[idProcess] = e.Message;
        }
        finally
        {
            await _historyNodeStateWriter.SetStateProcessAsync(
                contextBpmnProcess.IdBpmnProcess,
                contextBpmnProcess.TokenProcess,
                stateRegistry,
                errorRegistry,
                _timeInitInstanse);
            IsProcessCancel = true;
        }

        _logger.LogDebug(
            "[BpmnEngine:ThreadBackground] End business... {IdBpmnProcess} {TokenProcess}",
            contextBpmnProcess.IdBpmnProcess,
            contextBpmnProcess.TokenProcess);
    }

    /// <summary>
    /// Цикл обхода дерева bpmn.
    /// </summary>
    /// <param name="contextBpmnProcess"><see cref="IContextBpmnProcess"/>.</param>
    /// <param name="processModel"><see cref="ProcessModel"/>.</param>
    /// <param name="nodeStateRegistry">Регистр состояния нод.</param>
    /// <param name="errorRegistry">Регистр ошибок.</param>
    /// <param name="eventQueue">Ноды на запуск.</param>
    /// <param name="ctsToken"><see cref="CancellationToken"/>.</param>
    /// <returns>Завершен bpmn процесс.</returns>
    internal virtual async Task<bool> RunEventLoopAsync(
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
            _logger.LogDebug("[BpmnEngine:RunEventLoopAsync] Init Current Id: {CurrentId}", currentId);

            var node = processModel.Nodes[currentId];
            nodeStateRegistry[currentId] = StatusNode.Works;

            await _historyNodeStateWriter.SetStateProcessAsync(
                contextBpmnProcess.IdBpmnProcess,
                contextBpmnProcess.TokenProcess,
                nodeStateRegistry,
                errorRegistry,
                _timeInitInstanse);

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

            await _historyNodeStateWriter.SetStateProcessAsync(
                contextBpmnProcess.IdBpmnProcess,
                contextBpmnProcess.TokenProcess,
                nodeStateRegistry,
                errorRegistry,
                _timeInitInstanse);

            if (nodes.Status == StatusNode.FailedCompleted || nodes.Status == StatusNode.AllBpmnProcessCompleted)
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