namespace BpmnDotNet.BPMNDiagram;

/// <summary>
/// Тип Bpmn фигур.
/// </summary>
public enum BpmnShapeType
{
    /// <summary>
    /// Неизвестный.
    /// </summary>
    None = 0,

    /// <summary>
    /// Фигуры имеющие ширину и высоту, в разметки как bpmndi:BPMNShape.
    /// </summary>
    BpmnShape = 1,

    /// <summary>
    /// Фигуры имеющие только точки, в разметки как  bpmndi:BPMNEdge.
    /// </summary>
    BpmnEdge = 2,
}