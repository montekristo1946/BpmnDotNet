using BpmnDotNet.Common.Models;

namespace BpmnDotNet.Common.BPMNDiagram;

/// <summary>
///В нотации секция: bpmndi:BPMNShape
/// </summary>
public class BpmnShape
{
    /// <summary>
    ///  На схеме это поле:id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///  На схеме это поле:bpmnElement (элемент из секции process).
    /// </summary>
    public string BpmnElement { get; set; }

    /// <summary>
    /// Тип фигуры.
    /// </summary>
    public ElementType Type { get; set; }

    /// <summary>
    /// Переменная описывающая координаты.
    /// </summary>
    public Bound[] Bounds { get; set; }
}