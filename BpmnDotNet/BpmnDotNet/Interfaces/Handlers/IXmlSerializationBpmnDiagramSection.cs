using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Interfaces.Handlers;

public interface IXmlSerializationBpmnDiagramSection
{
    /// <summary>
    /// Сериализует секцию bpmn:process.
    /// </summary>
    /// <param name="pathDiagram"></param>
    /// <returns></returns>
    BpmnPlane LoadXmlBpmnDiagram(string pathDiagram);
}