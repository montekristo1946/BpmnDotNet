using BpmnDotNet.Abstractions.Elements;

namespace BpmnDotNet.Abstractions.Handlers;

/// <summary>
///     Сериализатор в объектный вид.
/// </summary>
public interface IXmlSerializationProcessSection
{
    /// <summary>
    ///     Сериализует секцию bpmn:process.
    /// </summary>
    /// <param name="pathDiagram"></param>
    /// <returns></returns>
    BpmnProcessDto LoadXmlProcessSection(string pathDiagram);
}