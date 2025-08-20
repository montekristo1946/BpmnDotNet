using System.Collections.Concurrent;
using System.Globalization;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Dto;
using BpmnDotNet.Elements;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Handlers;

internal class BusinessProcess : IBusinessProcess, IDisposable
{
    public BusinessProcessJobStatus JobStatus { get; private set; }

    private readonly IContextBpmnProcess _contextBpmnProcess;
    private readonly ILogger<IBusinessProcess> _logger;
    private readonly BpmnProcessDto _bpmnShema;
    private readonly IPathFinder _pathFinder;

    /// <summary>
    /// Handlers для обработки.
    /// </summary>
    private readonly ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> _handlers;

    /// <summary>
    /// Хранилище сообщений
    /// </summary>
    private readonly ConcurrentDictionary<Type, object> _messagesStore = new();

    /// <summary>
    /// Очередь для вызовов.
    /// </summary>
    private readonly ConcurrentDictionary<string, NodeTaskStatus> _nodeStateRegistry = new();


    private readonly CancellationTokenSource _cts;
    private bool _idDispose = false;

    private readonly AutoResetEvent _eventsHolder = new(false);

    public BusinessProcess(IContextBpmnProcess contextBpmnProcess,
        ILogger<IBusinessProcess> logger,
        BpmnProcessDto bpmnShema, IPathFinder pathFinder,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers,
        TimeSpan timeout)
    {
        _contextBpmnProcess = contextBpmnProcess ?? throw new ArgumentNullException(nameof(contextBpmnProcess));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bpmnShema = bpmnShema ?? throw new ArgumentNullException(nameof(bpmnShema));
        _pathFinder = pathFinder ?? throw new ArgumentNullException(nameof(pathFinder));
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));

        _cts = new CancellationTokenSource(timeout);
        var task = Task.Run(() => ThreadBackground(_cts.Token), _cts.Token);
        JobStatus = new BusinessProcessJobStatus()
        {
            ProcessingStaus = ProcessingStaus.Works,
            IdBpmnProcess = contextBpmnProcess.IdBpmnProcess,
            TokenProcess = contextBpmnProcess.TokenProcess,
            ProcessTask = task,
            Process = this
        };

    }


    public void Dispose()
    {
        if (_idDispose)
            return;

        _cts.Dispose();
        _idDispose = true;
    }

    public bool AddMessageToQueue(Type messageType, object message)
    {
        try
        {
            _messagesStore.AddOrUpdate(
                key: messageType,
                addValueFactory: _ => message,
                updateValueFactory: (key, oldMessage) => message);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        return false;
    }

    public void ForceCancel()
    {
        _cts.Cancel();
    }

    /// <summary>
    /// Главный поток процесса.
    /// </summary>
    /// <param name="ctsToken"></param>
    /// <returns></returns>
    private Task ThreadBackground(CancellationToken ctsToken)
    {
        _logger.LogDebug("[ThreadBackground] Starting business process... {IdBpmnProcess} {TokenProcess}",
            _contextBpmnProcess.IdBpmnProcess, _contextBpmnProcess.TokenProcess);

        try
        {
            AddStartEvent();
            var forcedRepetition = TimeSpan.FromMilliseconds(1000);

            while (!ctsToken.IsCancellationRequested)
            {
                UpdateParallelGatewayState();
                CheckMessagesStore();
                foreach (KeyValuePair<string, NodeTaskStatus> nodeState in _nodeStateRegistry)
                {
                    var isPending = nodeState.Value.ProcessingStaus == ProcessingStaus.Pending;
                    if (!isPending)
                        continue;

                    var taskNode = ExecutionNodes(nodeState.Key, ctsToken);
                    NodeRegistryChangeState(nodeState.Key, ProcessingStaus.Works, taskNode);
                }

                _eventsHolder.WaitOne(forcedRepetition);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug(
                "Command Stop. Business process is stopped. IdBpmnProcess: {IdBpmnProcess} TokenProcess:{TokenProcess}",
                _contextBpmnProcess.IdBpmnProcess, _contextBpmnProcess.TokenProcess);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        _logger.LogDebug("[ThreadBackground] End business process... {IdBpmnProcess} {TokenProcess}",
            _contextBpmnProcess.IdBpmnProcess, _contextBpmnProcess.TokenProcess);

        JobStatus.ProcessingStaus = ProcessingStaus.Complete;
        return Task.CompletedTask;
    }

    private void CheckMessagesStore()
    {
        foreach (KeyValuePair<string, NodeTaskStatus> nodeState in _nodeStateRegistry)
        {
            if (nodeState.Value.ProcessingStaus != ProcessingStaus.WaitingReceivedMessage)
                continue;

            var typeMessageInNode = GetTypeNameMessage(_contextBpmnProcess, nodeState.Key);
            var resGet = _messagesStore.TryGetValue(typeMessageInNode, out var message);
            if (!resGet || message is null)
            {
                continue;
            }

            var resAddMessage = AddMessageInContext(_contextBpmnProcess, typeMessageInNode, message);
            if (resAddMessage)
            {
                NodeRegistryChangeState(nodeState.Key, ProcessingStaus.Pending, null);
            }
        }
    }

    private bool AddMessageInContext(IContextBpmnProcess context, Type typeMessage, object? message)
    {
        if (message is null)
        {
            _logger.LogWarning(
                "[AddMessageInContext] received null message, IdBpmnProcess:{IdBpmnProcess}, TokenProcess:{TokenProcess}",
                context.IdBpmnProcess, context.TokenProcess);
            return false;
        }

        var messageReceiveTask = context as IMessageReceiveTask;

        if (messageReceiveTask is null)
            throw new InvalidOperationException(
                $"[AddMessageInContext] Fail Get context IMessageReceiveTask Dictionary, " +
                $"From node {typeMessage} {context.IdBpmnProcess} {context.TokenProcess}");

        var dic = messageReceiveTask.ReceivedMessage;
        var resSet = dic.TryAdd(typeMessage, message);

        if (!resSet)
            throw new InvalidOperationException(
                $"[AddMessageInContext] Fail Set {typeMessage}, From {context.IdBpmnProcess} {context.TokenProcess}");

        return true;
    }

    private Type GetTypeNameMessage(IContextBpmnProcess context, string nodeIdElement)
    {
        var messageReceiveTask = context as IMessageReceiveTask;

        if (messageReceiveTask is null)
            throw new InvalidOperationException($"[GetTypeMessage] Fail Get context IMessageReceiveTask Dictionary, " +
                                                $"From node {nodeIdElement} {context.IdBpmnProcess} {context.TokenProcess}");

        var dic = messageReceiveTask.RegistrationMessagesType;

        var resGet = dic.TryGetValue(nodeIdElement, out var typeName);

        if (!resGet || typeName is null)
            throw new InvalidOperationException($"[GetTypeMessage] Fail Get Message Name, " +
                                                $"From node {nodeIdElement} {context.IdBpmnProcess} {context.TokenProcess}");

        return typeName;
    }

    /// <summary>
    ///  Ожидающие путь, проверим что выполнили перед ними все flow.
    /// </summary>
    private void UpdateParallelGatewayState()
    {
        foreach (KeyValuePair<string, NodeTaskStatus> nodeState in _nodeStateRegistry)
        {
            if (nodeState.Value.ProcessingStaus != ProcessingStaus.WaitingCompletedWays)
                continue;

            var currentNode = GetIElement(nodeState.Key);
            var incomingPath = GetIncomingPath(currentNode);

            var checkCalls = incomingPath.All(p =>
            {
                var resGet = _nodeStateRegistry.TryGetValue(p, out var nodeJobStatus);
                return resGet && nodeJobStatus is not null && nodeJobStatus.ProcessingStaus == ProcessingStaus.Complete;
            });

            if (checkCalls)
            {
                NodeRegistryChangeState(nodeState.Key, ProcessingStaus.Pending, null);
            }
        }
    }

    private void AddStartEvent()
    {
        var allElements = _bpmnShema.ElementsFromBody;
        var currentNodes = _pathFinder.GetStartEvent(allElements);

        foreach (var node in currentNodes)
        {
            NodeRegistryChangeState(node.IdElement, ProcessingStaus.Pending, null);
            _logger.LogDebug($"[AddStartEvent] init Node: {node.IdElement} State: {ProcessingStaus.Pending}");
        }
    }

    private Task ExecutionNodes(string nodeId, CancellationToken ctsToken)
    {
        var retTask = Task.Run(async void () =>
        {
            try
            {
                var handler = GetHandler(nodeId);
                await handler(_contextBpmnProcess, ctsToken);

                NodeRegistryChangeState(nodeId, ProcessingStaus.Complete);
                FillFlowNodesToCompleted(nodeId);
                FillNextNodesToPending(nodeId);

                CheckFinalProcessing(nodeId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug(
                    "Command Stop. Node: {NodeId}. IdBpmnProcess: {IdBpmnProcess} TokenProcess:{TokenProcess}",
                    nodeId, _contextBpmnProcess.IdBpmnProcess, _contextBpmnProcess.TokenProcess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"{ex.Message} {nodeId}, {_contextBpmnProcess.IdBpmnProcess}, {_contextBpmnProcess.TokenProcess}");
                NodeRegistryChangeState(nodeId, ProcessingStaus.Failed);
            }
            finally
            {
                _eventsHolder.Set();
            }
        }, ctsToken);

        return retTask;
    }


    private void NodeRegistryChangeState(string nodeId, ProcessingStaus staus, Task? taskRunNode = null)
    {
        var stateNew = new NodeTaskStatus()
        {
            ProcessingStaus = staus,
            IdNode = nodeId,
            TaskRunNode = taskRunNode
        };

        _nodeStateRegistry.AddOrUpdate(
            key: nodeId,
            addValueFactory: _ => stateNew,
            updateValueFactory: (keyOld, oldMessage) =>
                new NodeTaskStatus()
                {
                    ProcessingStaus = staus,
                    IdNode = keyOld,
                    TaskRunNode = taskRunNode ?? oldMessage.TaskRunNode,
                }
        );
    }

    private IElement GetIElement(string nodeId)
    {
        var allElements = _bpmnShema.ElementsFromBody;
        var currentNode = allElements.FirstOrDefault(n => n.IdElement == nodeId) ??
                          throw new InvalidOperationException($"[GetIElement] Not found item:{nodeId}");
        return currentNode;
    }

    private void CheckFinalProcessing(string currentNodeId)
    {
        var currentNode = GetIElement(currentNodeId);
        if (currentNode.ElementType == ElementType.EndEvent)
        {
            _logger.LogDebug("[CheckFinalProcessing] EndEvent completed");
            _cts.Cancel();
            _eventsHolder.Set();
        }
    }

    private void FillNextNodesToPending(string currentNodeId)
    {
        var allElements = _bpmnShema.ElementsFromBody;
        var currentNode = GetIElement(currentNodeId);

        var nextNodes = _pathFinder.GetNextNode(allElements, [currentNode], _contextBpmnProcess);

        var sortedNoes = EliminateDuplicates(nextNodes);
        foreach (var node in sortedNoes)
        {
            var state = node.ElementType switch
            {
                ElementType.StartEvent => ProcessingStaus.Pending,
                ElementType.EndEvent => ProcessingStaus.Pending,
                ElementType.SequenceFlow => ProcessingStaus.Pending,
                ElementType.ExclusiveGateway => ProcessingStaus.Pending,
                ElementType.ServiceTask => ProcessingStaus.Pending,
                ElementType.SendTask => ProcessingStaus.Pending,
                ElementType.SubProcess => ProcessingStaus.Pending,
                ElementType.ReceiveTask => ProcessingStaus.WaitingReceivedMessage,
                ElementType.ParallelGateway => ProcessingStaus.WaitingCompletedWays,
                _ => throw new NotImplementedException(
                    $"[FillNextNodesToPending] Not find ImplementedException {node.ElementType}")
            };
            _logger.LogDebug($"[FillNextNodesToPending] init Node: {node.IdElement} State: {state}");
            NodeRegistryChangeState(node.IdElement, state, null);
        }
    }

    private void FillFlowNodesToCompleted(string currentNodeId)
    {
        var currentNode = GetIElement(currentNodeId);

        if (currentNode.ElementType == ElementType.ExclusiveGateway)
        {
            var idNode = _pathFinder.GetConditionRouteWithExclusiveGateWay(_contextBpmnProcess, currentNode);
            NodeRegistryChangeState(idNode, ProcessingStaus.Complete, null);
            _logger.LogDebug($"[FillFlowNodesToCompleted] init Node: {idNode} State: {ProcessingStaus.Complete}");
            return;
        }

        var flowsId = GetOutgoingPath(currentNode);
        foreach (var flow in flowsId)
        {
            NodeRegistryChangeState(flow, ProcessingStaus.Complete, null);
            _logger.LogDebug($"[FillFlowNodesToCompleted] init Node: {flow} State: {ProcessingStaus.Complete}");
        }
    }

    private string[] GetOutgoingPath(IElement currentNode)
    {
        if (currentNode is IOutgoingPath outgoingPath)
        {
            return outgoingPath.Outgoing;
        }

        return [];
    }

    private string[] GetIncomingPath(IElement currentNode)
    {
        if (currentNode is IIncomingPath incomingPath)
        {
            return incomingPath.Incoming;
        }

        return [];
    }

    private IElement[] EliminateDuplicates(IElement[] nodes)
    {
        var result = new List<IElement>();
        foreach (var node in nodes)
        {
            var resGet = _nodeStateRegistry.TryGetValue(node.IdElement, out var nodeJobStatus);
            if (resGet && nodeJobStatus is not null &&
                nodeJobStatus.ProcessingStaus == ProcessingStaus.Works) //случай когда параллельный шлюз уже запущен
            {
                continue;
            }

            result.Add(node);
        }

        return result.ToArray();
    }

    private Func<IContextBpmnProcess, CancellationToken, Task> GetHandler(string nodeIdElement)
    {
        var resGet = _handlers.TryGetValue(nodeIdElement, out var value);

        if (!resGet || value is null)
            return MoqHandler;

        return value;
    }

    private Task MoqHandler(IContextBpmnProcess contextBpmnProcess, CancellationToken ctsToken)
    {
        _logger.LogDebug("[MoqHandler] Calling handler");
        return Task.CompletedTask;
    }

}