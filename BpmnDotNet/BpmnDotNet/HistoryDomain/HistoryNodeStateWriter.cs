namespace BpmnDotNet.Handlers;

using System.Collections.Concurrent;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.Dto;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using BpmnDotNet.HistoryDomain.Abstractions;
using BpmnDotNet.HistoryDomain.Dto;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
internal class HistoryNodeStateWriter : IHistoryNodeStateWriter
{
    private readonly IElasticClientSetDataAsync _elasticClient;
    private readonly ILogger<HistoryNodeStateWriter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryNodeStateWriter"/> class.
    /// </summary>
    /// <param name="elasticClient">elasticClient.</param>
    /// <param name="logger">logger.</param>
    public HistoryNodeStateWriter(IElasticClientSetDataAsync elasticClient, ILogger<HistoryNodeStateWriter> logger)
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task SetStateProcessAsync(
        string idBpmnProcess,
        string tokenProcess,
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ConcurrentDictionary<string, string> errorRegistry,
        long dateFromInitInstance)
    {
        ArgumentNullException.ThrowIfNull(nodeStateRegistry);
        ArgumentNullException.ThrowIfNull(errorRegistry);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dateFromInitInstance);

        if (string.IsNullOrWhiteSpace(idBpmnProcess))
        {
            throw new ArgumentNullException(nameof(idBpmnProcess));
        }

        if (string.IsNullOrWhiteSpace(tokenProcess))
        {
            throw new ArgumentNullException(nameof(tokenProcess));
        }

        var nodeJobStatus = nodeStateRegistry.Select(p => new NodeJobStatus()
        {
            IdNode = p.Key,
            StatusType = p.Value,
        }).ToArray();

        var processingStaus = CalculateProcessingStaus(nodeJobStatus);

        var historyNodeState = new HistoryNodeState()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess,
            DateCreated = dateFromInitInstance,
            DateLastModified = DateTime.Now.Ticks,
            ProcessStatus = processingStaus,
            NodeStaus = nodeJobStatus,
            ArrayMessageErrors = errorRegistry.Values.ToArray(),
        };


        var resSetData = await _elasticClient.SetDataAsync(historyNodeState);

        if (resSetData is false)
        {
            _logger.LogError(
                "[HistoryNodeStateWriter:SetStateProcessAsync] Failed to set history node state: {IdBpmnProcess} {TokenProcess}",
                idBpmnProcess,
                tokenProcess);
        }
    }

    private ProcessStatus CalculateProcessingStaus(NodeJobStatus[] nodeJobStatus)
    {
        var errors = nodeJobStatus.FirstOrDefault(p => p.StatusType == StatusNode.FailedCompleted);
        if (errors is not null)
        {
            return ProcessStatus.Error;
        }

        var worksNode = nodeJobStatus.FirstOrDefault(p => p.StatusType == StatusNode.Works);
        if (worksNode is not null)
        {
            return ProcessStatus.Works;
        }

        var completed = nodeJobStatus.FirstOrDefault(p => p.StatusType == StatusNode.AllBpmnProcessCompleted);
        if (completed is not null)
        {
            return ProcessStatus.Completed;
        }

        return ProcessStatus.None;
    }

    // /// <inheritdoc/>
    // public async Task SetStateProcessAsync(
    //     string idBpmnProcess,
    //     string tokenProcess,
    //     NodeJobStatus[] nodeStateRegistry,
    //     string[] arrayMessageErrors,
    //     bool isCompleted,
    //     long dateFromInitInstance)
    // {
    //     if (string.IsNullOrWhiteSpace(idBpmnProcess))
    //     {
    //         throw new ArgumentNullException(nameof(idBpmnProcess));
    //     }
    //
    //     if (string.IsNullOrWhiteSpace(tokenProcess))
    //     {
    //         throw new ArgumentNullException(nameof(tokenProcess));
    //     }
    //
    //     ArgumentNullException.ThrowIfNull(nodeStateRegistry);
    //     ArgumentNullException.ThrowIfNull(arrayMessageErrors);
    //
    //     var nodeJobStatus = nodeStateRegistry.Select(p => new NodeJobStatus()
    //     {
    //         IdNode = p.IdNode,
    //         StatusType = p.StatusType,
    //     }).ToArray();
    //
    //     var processingStaus = CalculateProcessingStaus(nodeStateRegistry, isCompleted);
    //
    //     var historyNodeState = new HistoryNodeState()
    //     {
    //         IdBpmnProcess = idBpmnProcess,
    //         TokenProcess = tokenProcess,
    //         NodeStaus = nodeJobStatus,
    //         ArrayMessageErrors = arrayMessageErrors,
    //         DateCreated = dateFromInitInstance,
    //         DateLastModified = DateTime.Now.Ticks,
    //         ProcessStatus = processingStaus,
    //     };
    //
    //     var resSetData = await _elasticClient.SetDataAsync(historyNodeState);
    //
    //     if (resSetData is false)
    //     {
    //         _logger.LogError($"[HistoryNodeStateWriter:SetStateProcessAsync] Failed to set history node state: {idBpmnProcess} {tokenProcess}");
    //     }
    // }
    //
    // /// <inheritdoc/>
    // public async Task SetStateProcessWithManualProcessStatus(
    //     string idBpmnProcess,
    //     string tokenProcess,
    //     NodeJobStatus[] nodeStateRegistry,
    //     string[] arrayMessageErrors,
    //     long dateFromInitInstance,
    //     ProcessStatus processStatus)
    // {
    //     if (string.IsNullOrWhiteSpace(idBpmnProcess))
    //     {
    //         throw new ArgumentNullException(nameof(idBpmnProcess));
    //     }
    //
    //     if (string.IsNullOrWhiteSpace(tokenProcess))
    //     {
    //         throw new ArgumentNullException(nameof(tokenProcess));
    //     }
    //
    //     ArgumentNullException.ThrowIfNull(nodeStateRegistry);
    //     ArgumentNullException.ThrowIfNull(arrayMessageErrors);
    //
    //     var nodeJobStatus = nodeStateRegistry.Select(p => new NodeJobStatus()
    //     {
    //         IdNode = p.IdNode,
    //         StatusType = p.StatusType,
    //     }).ToArray();
    //
    //     var historyNodeState = new HistoryNodeState()
    //     {
    //         IdBpmnProcess = idBpmnProcess,
    //         TokenProcess = tokenProcess,
    //         NodeStaus = nodeJobStatus,
    //         ArrayMessageErrors = arrayMessageErrors,
    //         DateCreated = dateFromInitInstance,
    //         DateLastModified = DateTime.Now.Ticks,
    //         ProcessStatus = processStatus,
    //     };
    //
    //     var resSetData = await _elasticClient.SetDataAsync(historyNodeState);
    //
    //     if (resSetData is false)
    //     {
    //         _logger.LogError($"[HistoryNodeStateWriter:SetStateProcessWithManualProcessStatus] Failed to set history node state: {idBpmnProcess} {tokenProcess}");
    //     }
    // }
    //
    // private ProcessStatus CalculateProcessingStaus(NodeJobStatus[] nodeStateRegistry, bool isCompleted)
    // {
    //     var errors = nodeStateRegistry.FirstOrDefault(p => p.StatusType == StatusType.Failed);
    //     if (errors is not null)
    //     {
    //         return ProcessStatus.Error;
    //     }
    //
    //     if (isCompleted)
    //     {
    //         return ProcessStatus.Completed;
    //     }
    //
    //     if (nodeStateRegistry.Any() is false)
    //     {
    //         return ProcessStatus.None;
    //     }
    //
    //     return ProcessStatus.Works;
    // }
}