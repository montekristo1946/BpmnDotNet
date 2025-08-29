using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Arm.Core.Handlers;

public class PlanePanelHandler : IPlanePanelHandler
{
    
    private readonly ILogger<PlanePanelHandler> _logger;
    private readonly IElasticClient  _elasticClient;
    private readonly ISvgConstructor _svgConstructor;

    public PlanePanelHandler(ILogger<PlanePanelHandler> logger, IElasticClient elasticClient, ISvgConstructor svgConstructor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _svgConstructor = svgConstructor ?? throw new ArgumentNullException(nameof(svgConstructor));
    }
    
    public async Task<string> GetPlane(string idProcess, SizeWindows sizeWindows)
    {
        var plane = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(idProcess) ?? new BpmnPlane();
        var svg = await _svgConstructor.CreatePlane(plane,sizeWindows);
       
        // var serialization = new XmlSerializationBpmnDiagramSection();
        // var diagram = serialization.LoadXmlBpmnDiagram(
        //     "/mnt/Disk_C/git/BpmnDotNet/BpmnDotNet/BpmnDotNet.Arm.Core.Tests/BpmnDiagram/diagram_1.bpmn");

        return svg;
    }
}