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

    public async Task<string> GetPlane(string idBpmnProcess, SizeWindows sizeWindows)
    {
        var plane = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(idBpmnProcess) ?? new BpmnPlane();
        var descriptors = await GetDescriptor(plane);
        var svg = await _svgConstructor.CreatePlane(plane, [], sizeWindows,descriptors);

        return svg;
    }

    private async Task<DescriptionData[]> GetDescriptor(BpmnPlane plane)
    {
        var result = new List<DescriptionData>(); 
        foreach (var planeShape in plane.Shapes)
        {
            var data = await _elasticClient.GetDataFromIdAsync<DescriptionData>(planeShape.BpmnElement);
            if (data != null)
            {
                result.Add(data);
            }
        }
        return result.ToArray();
    }

    public async Task<string> GetColorPlane(string idUpdateNodeJobStatus, SizeWindows sizeWindows)
    {
        var historyNodeState = await _elasticClient.GetDataFromIdAsync<HistoryNodeState>(idUpdateNodeJobStatus,
            [nameof(HistoryNodeState.ArrayMessageErrors)]) ?? new HistoryNodeState();

        var plane = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(historyNodeState.IdBpmnProcess) ?? new BpmnPlane();
        var descriptors = await GetDescriptor(plane);
        var svg = await _svgConstructor.CreatePlane(plane, historyNodeState.NodeStaus, sizeWindows,descriptors);

        return svg;
    }

}