namespace BpmnDotNet.BPMNDiagram.Abstractions;

/// <summary>
/// Описывает все фигуры xml разметки.
/// </summary>
public interface IBpmnShape
{
    /// <summary>
    ///     Gets уникальное имя для схемы BPMNShape. На схеме это поле:id.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    ///    Gets поле-дескриптор.
    /// </summary>
    public BpmnShapeType TypeBpmnShape { get; init; }

    /// <summary>
    ///     Gets называние элемент из секции process. На схеме это поле:bpmnElement.
    /// </summary>
    public string BpmnElement { get; init; }
}