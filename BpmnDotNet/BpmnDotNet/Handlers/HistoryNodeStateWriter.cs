using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Dto;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Handlers;

internal class HistoryNodeStateWriter : IHistoryNodeStateWriter
{
    private readonly IElasticClientSetDataAsync _elasticClient;
    private readonly ILogger<HistoryNodeStateWriter> _logger;

    public HistoryNodeStateWriter(IElasticClientSetDataAsync elasticClient, ILogger<HistoryNodeStateWriter> logger)
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SetStateProcess(string idBpmnProcess,
        string tokenProcess,
        NodeJobStatus[] nodeStateRegistry,
        string[] arrayMessageErrors,
        bool isCompleted,
        long dateFromInitInstance)
    {
        if (string.IsNullOrWhiteSpace(idBpmnProcess))
        {
            throw new ArgumentNullException(nameof(idBpmnProcess));
        }
        if (string.IsNullOrWhiteSpace(tokenProcess))
        {
            throw new ArgumentNullException(nameof(tokenProcess));
        }

        ArgumentNullException.ThrowIfNull(nodeStateRegistry);
        ArgumentNullException.ThrowIfNull(arrayMessageErrors);

        var nodeJobStatus = nodeStateRegistry.Select(p => new NodeJobStatus()
        {
            IdNode = p.IdNode,
            StatusType = p.StatusType,
        }).ToArray();

        var processingStaus = CalculateProcessingStaus(nodeStateRegistry, isCompleted);

        var historyNodeState = new HistoryNodeState()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess,
            NodeStaus = nodeJobStatus,
            ArrayMessageErrors = arrayMessageErrors,
            DateCreated = dateFromInitInstance,
            DateLastModified = DateTime.Now.Ticks,
            ProcessStatus = processingStaus,
        };

        var resSetData = await _elasticClient.SetDataAsync(historyNodeState);

        if (resSetData is false)
        {
            _logger.LogError($"[SetStateProcess] Failed to set history node state: {idBpmnProcess} {tokenProcess}");
        }
    }

    private ProcessStatus CalculateProcessingStaus(NodeJobStatus[] nodeStateRegistry, bool isCompleted)
    {
        var errors = nodeStateRegistry.FirstOrDefault(p => p.StatusType == StatusType.Failed);
        if (errors is not null)
        {
            return ProcessStatus.Error;
        }
        
        if (isCompleted)
        {
            return ProcessStatus.Completed;
        }

        if (nodeStateRegistry.Any() is false)
        {
            return ProcessStatus.None;
        }

        
        return ProcessStatus.Works;
    }
}