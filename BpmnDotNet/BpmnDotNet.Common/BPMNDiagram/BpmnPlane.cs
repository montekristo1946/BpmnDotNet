namespace BpmnDotNet.Common.BPMNDiagram;

/// <summary>
/// В нотации секция: bpmndi:BPMNPlane.
/// </summary>
public record BpmnPlane
{
    /// <summary>
    /// На схеме это поле:id
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// На схеме это поле: bpmnElement (Id всего процесса)
    /// </summary>
    public string IdBpmnProcess { get; init; }= string.Empty;

    /// <summary>
    /// Массив фигур для отрисовки.
    /// </summary>
    public BpmnShape[] Shapes { get; init; } = [];
}