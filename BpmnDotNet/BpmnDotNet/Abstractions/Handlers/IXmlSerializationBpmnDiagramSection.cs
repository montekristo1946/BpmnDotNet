using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Abstractions.Handlers;

public interface IXmlSerializationBpmnDiagramSection
{
    /// <summary>
    ///     Сериализует секцию bpmn:process.
    /// </summary>
    /// <param name="pathDiagram"></param>
    /// <returns></returns>
    BpmnPlane LoadXmlBpmnDiagram(string pathDiagram);
}