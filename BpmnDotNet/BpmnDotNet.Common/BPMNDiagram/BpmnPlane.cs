namespace BpmnDotNet.Common.BPMNDiagram;

/// <summary>
/// В нотации секция: bpmndi:BPMNPlane.
/// </summary>
public class BpmnPlane
{
    /// <summary>
    /// На схеме это поле:id
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// На схеме это поле: bpmnElement (Id всего процесса)
    /// </summary>
    public string BpmnElement { get; set; }
    
    /// <summary>
    /// Массив фигур для отрисовки.
    /// </summary>
    BpmnShape[] Shape { get; set; }
}