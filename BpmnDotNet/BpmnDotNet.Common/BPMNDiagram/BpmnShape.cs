namespace BpmnDotNet.Common.BPMNDiagram;

using BpmnDotNet.Common.Models;

/// <summary>
///     В нотации секция: bpmndi:BPMNShape.
/// </summary>
public record BpmnShape
{
    /// <summary>
    ///     Gets уникальное имя для схемы BPMNShape. На схеме это поле:id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    ///     Gets называние элемент из секции process. На схеме это поле:bpmnElement.
    /// </summary>
    public string BpmnElement { get; init; } = string.Empty;

    /// <summary>
    ///     Gets тип фигуры.
    /// </summary>
    public ElementType Type { get; init; } = ElementType.None;

    /// <summary>
    ///     Gets переменная описывающая координаты левого угла фигуры, (массив например для Flow).
    /// </summary>
    public Bound[] Bounds { get; init; } = [];

    /// <summary>
    ///     Gets текст в элементах. Описывает name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Gets координаты текста. На схеме это bpmndi:BPMNLabel.
    /// </summary>
    public Bound BpmnLabel { get; init; } = new();
}