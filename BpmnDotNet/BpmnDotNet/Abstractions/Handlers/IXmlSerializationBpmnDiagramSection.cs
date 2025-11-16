namespace BpmnDotNet.Abstractions.Handlers;

using BpmnDotNet.Common.Entities;

/// <summary>
/// Сериализатор визуальную часть диаграмм.
/// </summary>
public interface IXmlSerializationBpmnDiagramSection
{
    /// <summary>
    ///     Сериализует секцию bpmn:process.
    /// </summary>
    /// <param name="pathDiagram">Путь до диаграмм.</param>
    /// <returns>Диаграмма в объектном виде.</returns>
    BpmnPlane LoadXmlBpmnDiagram(string pathDiagram);
}