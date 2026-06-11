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
    private Task _threadBackground = null!;
    private readonly SemaphoreSlim _semaphore = new(0, 1);
    private readonly ConcurrentQueue<Token> _eventQueue = new();

    /// <summary>
    ///     Очередь для вызовов.
    /// </summary>
    private readonly ConcurrentDictionary<string, StatusNode> _nodeStateRegistry = new();

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
        if (_threadBackground != null)
        {
            throw new InvalidOperationException(
                "[BpmnEngine:StartProcessAsync] The thread background has already been started.");
        }

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

        try
        {
            while (!ctsToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(ctsToken);
                var isCompletedBackground = await RunEventLoopAsync(contextBpmnProcess, ctsToken, processModel);
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
        CancellationToken ctsToken,
        ProcessModel processModel)
    {
        while (_eventQueue.Count > 0 && !ctsToken.IsCancellationRequested)
        {
            var resGetToken = _eventQueue.TryDequeue(out var token);
            if (!resGetToken || token == null)
            {
                _logger.LogError("[BpmnEngine:RunEventLoopAsync] No events queued");
                return false;
            }

            var currentId = token.CurrentNodeId;
            var node = processModel.Nodes[currentId];
            _nodeStateRegistry.AddOrUpdate(currentId, StatusNode.WorksNode, (k, v) => v);

            // TODO: Делать snapshot _nodeStateRegistry в ElasticSearch
            var nodes = await node.ExecuteAsync(processModel, contextBpmnProcess, _nodeStateRegistry, ctsToken);
            if (nodes is null)
            {
                throw new InvalidOperationException("[BpmnEngine:RunEventLoopAsync] ExecuteAsync returned null.");
            }

            _nodeStateRegistry.AddOrUpdate(currentId, nodes.Status, (k, v) => nodes.Status);
            // TODO: Делать snapshot _nodeStateRegistry в ElasticSearch


            if (nodes.Status == StatusNode.FailedCompletedNode || nodes.Status == StatusNode.AllBpmnProcessCompleted)
            {
                return true;
            }

            nodes.Tokens.ToList().ForEach(p => _eventQueue.Enqueue(p));
        }

        return false;
    }


    /// <inheritdoc/>
    public bool AddMessageToQueue(Type messageType, object message)
    {
        _semaphore.Release();

        throw new NotImplementedException();
    }
}