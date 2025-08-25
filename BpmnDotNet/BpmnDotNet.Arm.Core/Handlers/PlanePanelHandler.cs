using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Handlers;

namespace BpmnDotNet.Arm.Core.Handlers;

public class PlanePanelHandler:IPlanePanelHandler
{
    public Task <BpmnPlane> GetPlane()
    {
        var serialization = new XmlSerializationBpmnDiagramSection();
        var diagram = serialization.LoadXmlBpmnDiagram("/mnt/Disk_C/git/BpmnDotNet/BpmnDotNet/BpmnDotNet.ElasticClient.Tests/BpmnDiagram/diagram_1.bpmn");
        
        return Task.FromResult(diagram);
    }
}