using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.BpmnEngineDomain;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <inheritdoc cref="StartProcessAsync" />
internal class BpmnEngine : IBpmnEngine
{
    private readonly ILogger<BpmnEngine> _logger;
    private Task _threadBackground = null!;
    private readonly SemaphoreSlim _semaphore = new(0, 1);
    private readonly ConcurrentQueue<Token> _eventQueue = new();

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

        _threadBackground = Task.Run(() => ThreadBackground(processModel, contextBpmnProcess, ct), ct);
        var jobStatus = new BusinessProcessJobStatusV2
        {
            StatusType = StatusBpmnEngine.Works,
            IdBpmnProcess = contextBpmnProcess.IdBpmnProcess,
            TokenProcess = contextBpmnProcess.TokenProcess,
            ProcessTask = _threadBackground,
            Process = this,
        };

        return Task.FromResult(jobStatus);
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

        while (!ctsToken.IsCancellationRequested)
        {
            await _semaphore.WaitAsync(ctsToken);
            await RunEventLoopAsync(contextBpmnProcess, ctsToken, processModel);
        }
    }

    private async Task RunEventLoopAsync(
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
                return;
            }

            try
            {
                var node = processModel.Nodes[token.CurrentNodeId];
                var nodes = await node.ExecuteAsync(contextBpmnProcess, ctsToken);

                nodes.ToList().ForEach(p => _eventQueue.Enqueue(p));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[BpmnEngine:RunEventLoopAsync] Exception");
            }
        }
    }


    public bool AddMessageToQueue(Type messageType, object message)
    {
        _semaphore.Release();

        throw new NotImplementedException();
    }
}