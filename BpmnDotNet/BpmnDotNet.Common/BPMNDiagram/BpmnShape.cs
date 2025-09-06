using BpmnDotNet.Common.Models;

namespace BpmnDotNet.Common.BPMNDiagram;

/// <summary>
///     В нотации секция: bpmndi:BPMNShape
/// </summary>
public record BpmnShape
{
    /// <summary>
    ///     Уникальное имя для схемы BPMNShape. На схеме это поле:id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    ///     Называние элемент из секции process. На схеме это поле:bpmnElement.
    /// </summary>
    public string BpmnElement { get; init; } = string.Empty;

    /// <summary>
    ///     Тип фигуры.
    /// </summary>
    public ElementType Type { get; init; } = ElementType.None;

    /// <summary>
    ///     Переменная описывающая координаты левого угла фигуры, (массив например для Flow)
    /// </summary>
    public Bound[] Bounds { get; init; } = [];

    /// <summary>
    ///     Текст в элементах. Описывает name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     Координаты текста. На схеме это bpmndi:BPMNLabel,
    /// </summary>
    public Bound BpmnLabel { get; init; } = new();
}