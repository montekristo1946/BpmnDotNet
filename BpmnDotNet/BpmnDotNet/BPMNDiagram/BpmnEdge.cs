namespace BpmnDotNet.BPMNDiagram;

using BpmnDotNet.BPMNDiagram.Abstractions;

/// <summary>
///     В нотации секция: bpmndi:BPMNEdge.
/// </summary>
public record BpmnEdge : IBpmnShape
{
    /// <inheritdoc />
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public BpmnShapeType TypeBpmnShape { get; init; } = BpmnShapeType.BpmnEdge;

    /// <inheritdoc/>
    public string BpmnElement { get; init; } = string.Empty;

    /// <summary>
    ///     Gets переменная описывающая координаты левого угла фигуры, (массив например для Flow).
    /// </summary>
    public Waypoint[] Waypoints { get; init; } = [];

    /// <summary>
    ///     Gets текст в элементах. Описывает name (в разметке хранится в bpmn:process: name).
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Gets координаты текста. На схеме это bpmndi:BPMNLabel.
    /// </summary>
    public Bound BpmnLabel { get; init; } = new();

    /// <summary>
    ///     Gets тип фигуры.
    /// </summary>
    public ElementType Type { get; init; } = ElementType.None;
}