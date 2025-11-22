namespace BpmnDotNet.Arm.Core.UiDomain.Services;

using BpmnDotNet.Arm.Core.UiDomain.Abstractions;
using BpmnDotNet.Arm.Core.UiDomain.Dto;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class ListProcessPanelHandler : IListProcessPanelHandler
{
    private readonly ILogger<ListProcessPanelHandler> _logger;
    private readonly IElasticClient _elasticClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListProcessPanelHandler"/> class.
    /// </summary>
    /// <param name="logger">logger.</param>
    /// <param name="elasticClient">elasticClient.</param>
    public ListProcessPanelHandler(ILogger<ListProcessPanelHandler> logger, IElasticClient elasticClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    /// <inheritdoc />
    public async Task<int> GetCountAllRows(string idActiveProcess, string[] processStatus)
    {
        if (string.IsNullOrEmpty(idActiveProcess))
        {
            return 0;
        }

        var res = await _elasticClient.GetCountHistoryNodeStateAsync(idActiveProcess, processStatus);

        return res;
    }

    /// <inheritdoc />
    public async Task<ListProcessPanelDto[]> GetPagesStates(
        string idBpmnProcess,
        int skip,
        int take,
        string[] processStatus)
    {
        if (string.IsNullOrEmpty(idBpmnProcess))
        {
            return [];
        }

        if (take == 0)
        {
            return [];
        }

        var historyNodes =
            await _elasticClient.GetHistoryNodeStateAsync(idBpmnProcess, skip, take, processStatus);

        var retArray = new List<ListProcessPanelDto>();
        foreach (var historyNode in historyNodes)
        {
            var listProcessPanelDto = new ListProcessPanelDto()
            {
                TokenProcess = historyNode.TokenProcess,
                DateCreated = new DateTime(historyNode.DateCreated),
                DateLastModified = new DateTime(historyNode.DateLastModified),
                IdBpmnProcess = historyNode.IdBpmnProcess,
                State = Map(historyNode.ProcessStatus),
                IdStorageHistoryNodeState = historyNode.Id,
            };
            retArray.Add(listProcessPanelDto);
        }

        return [.. retArray];
    }

    /// <inheritdoc />
    public async Task<string[]> GetErrors(string idUpdateNodeJobStatus)
    {
        var historyNodeState = await _elasticClient
            .GetDataFromIdAsync<HistoryNodeState>(
                idUpdateNodeJobStatus,
                [nameof(HistoryNodeState.NodeStaus)]) ?? new HistoryNodeState();

        return historyNodeState?.ArrayMessageErrors ?? [];
    }

    /// <inheritdoc />
    public async Task<ListProcessPanelDto[]> GetHistoryNodeFromTokenMaskAsync(string idBpmnProcess, string filterToken, int sizeSample)
    {
        var historyNodes = await _elasticClient.GetHistoryNodeFromTokenMaskAsync(idBpmnProcess, filterToken, sizeSample);
        var retArray = new List<ListProcessPanelDto>();

        foreach (var historyNode in historyNodes)
        {
            var listProcessPanelDto = new ListProcessPanelDto()
            {
                TokenProcess = historyNode.TokenProcess,
                DateCreated = new DateTime(historyNode.DateCreated),
                DateLastModified = new DateTime(historyNode.DateLastModified),
                IdBpmnProcess = historyNode.IdBpmnProcess,
                State = Map(historyNode.ProcessStatus),
                IdStorageHistoryNodeState = historyNode.Id,
            };
            retArray.Add(listProcessPanelDto);
        }

        return [.. retArray];
    }

    private ProcessState Map(ProcessStatus argStatusType)
    {
        return argStatusType switch
        {
            ProcessStatus.Completed => ProcessState.Completed,
            ProcessStatus.Error => ProcessState.Error,
            ProcessStatus.Works => ProcessState.Works,
            ProcessStatus.None => ProcessState.None,
            _ => ProcessState.Works,
        };
    }
}