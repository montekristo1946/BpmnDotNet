namespace BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Дто описывающая в объектном виде  структуру bpmn документа.
/// </summary>
/// <param name="idBpmnProcess">idBpmnProcess.</param>
/// <param name="elementsFromBody">Элементы схемы.</param>
internal class BpmnProcessDto(string idBpmnProcess, IElement[] elementsFromBody)
{
    /// <summary>
    /// Gets iD процесса.
    /// </summary>
    public string IdBpmnProcess { get; init; } = idBpmnProcess;

    /// <summary>
    /// Gets блоки bpmn схемы.
    /// </summary>
    public IElement[] ElementsFromBody { get; init; } = elementsFromBody;
}