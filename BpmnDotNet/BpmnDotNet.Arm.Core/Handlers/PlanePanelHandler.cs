using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Arm.Core.Handlers;

public class PlanePanelHandler : IPlanePanelHandler
{

    private readonly ILogger<PlanePanelHandler> _logger;
    private readonly IElasticClient _elasticClient;
    private readonly ISvgConstructor _svgConstructor;

    public PlanePanelHandler(ILogger<PlanePanelHandler> logger, IElasticClient elasticClient, ISvgConstructor svgConstructor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _svgConstructor = svgConstructor ?? throw new ArgumentNullException(nameof(svgConstructor));
    }

    public async Task<string> GetPlane(string IdBpmnProcess, SizeWindows sizeWindows)
    {
        var plane = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(IdBpmnProcess) ?? new BpmnPlane();
        var svg = await _svgConstructor.CreatePlane(plane, sizeWindows);

        return svg;
    }

    public async Task<string> GetColorPlane(string idUpdateNodeJobStatus, SizeWindows sizeWindows)
    {
        var historyNodeState = await _elasticClient.GetDataFromIdAsync<HistoryNodeState>(idUpdateNodeJobStatus,
            [nameof(HistoryNodeState.ArrayMessageErrors)]) ?? new HistoryNodeState();

        var plane = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(historyNodeState.IdBpmnProcess) ?? new BpmnPlane();

        var svg = await _svgConstructor.CreatePlane(plane, historyNodeState.NodeStaus, sizeWindows);

        return svg;
    }

}