using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Interfaces.Handlers;

/// <summary>
/// Сериализатор в обьектынй вид.
/// </summary>
public interface IXmlSerializationProcessSection
{
    /// <summary>
    /// Сериализует секцию bpmn:process.
    /// </summary>
    /// <param name="pathDiagram"></param>
    /// <returns></returns>
    BpmnProcessDto LoadXmlProcessSection(string pathDiagram);
}