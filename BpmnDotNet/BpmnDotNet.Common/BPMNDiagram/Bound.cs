namespace BpmnDotNet.Common.BPMNDiagram;

/// <summary>
/// В нотации секция: dc:Bounds (границы)
/// </summary>
public class Bound
{
    /// <summary>
    /// На схеме это поле: Bounds-x.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// На схеме это поле: Bounds-y.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// На схеме это поле: Bounds-width.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// На схеме это поле: Bounds-height.
    /// </summary>
    public int Height { get; set; }
}