namespace BpmnDotNet.Abstractions.Handlers;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
///     Сериализатор в объектный вид процесс выполнения.
/// </summary>
internal interface IXmlSerializationProcessSection
{
    /// <summary>
    ///     Сериализует секцию bpmn:process.
    /// </summary>
    /// <param name="pathDiagram">Путь до диаграмм.</param>
    /// <returns>Диаграмма в объектном виде.</returns>
    public BpmnProcessDto LoadXmlProcessSection(string pathDiagram);
}