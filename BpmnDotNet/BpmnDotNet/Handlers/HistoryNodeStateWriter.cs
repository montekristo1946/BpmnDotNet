using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Dto;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Handlers;

internal class HistoryNodeStateWriter:IHistoryNodeStateWriter
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<HistoryNodeStateWriter> _logger;

    public HistoryNodeStateWriter(IElasticClient elasticClient, ILogger<HistoryNodeStateWriter> logger)
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SetStateProcess(string idBpmnProcess, string tokenProcess, NodeTaskStatus[] nodeStateRegistry)
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

        var nodeJobStatus = nodeStateRegistry.Select(p => new NodeJobStatus()
        {
            IdNode = p.IdNode,
            ProcessingStaus = p.ProcessingStaus,
        }).ToArray();

        var historyNodeState = new HistoryNodeState()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess,
            NodeStaus = nodeJobStatus,
        };

        var resSetData = await _elasticClient.SetDataAsync(historyNodeState);

        if (resSetData is false)
        {
            _logger.LogError($"[SetStateProcess] Failed to set history node state: {idBpmnProcess} {tokenProcess}");
        }
    }
}