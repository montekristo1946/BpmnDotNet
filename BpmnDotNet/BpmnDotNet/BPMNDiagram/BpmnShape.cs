using System.Text.Json.Serialization;

namespace BpmnDotNet.BPMNDiagram;

using BpmnDotNet.BPMNDiagram.Abstractions;

/// <summary>
///     В нотации секция: bpmndi:BPMNShape.
/// </summary>
public record BpmnShape : IBpmnShape
{
    /// <inheritdoc />
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public BpmnShapeType TypeBpmnShape { get; init; } = BpmnShapeType.BpmnShape;

    /// <inheritdoc/>
    public string BpmnElement { get; init; } = string.Empty;

    /// <summary>
    ///     Gets тип фигуры.
    /// </summary>
    public ElementType Type { get; init; } = ElementType.None;

    /// <summary>
    ///     Gets переменная описывающая координаты левого угла фигуры.
    /// </summary>
    public Bound Bounds { get; init; } = new Bound();

    /// <summary>
    ///     Gets текст в элементах. Описывает name (в разметке хранится в bpmn:process: name).
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Gets координаты текста. На схеме это bpmndi:BPMNLabel.
    /// </summary>
    public Bound BpmnLabel { get; init; } = new();
}