using System;

namespace BpmnDotNet.Common.BPMNDiagram;

/// <summary>
/// В нотации секция: dc:Bounds (границы)
/// </summary>
public record Bound
{
    /// <summary>
    /// На схеме это поле: Bounds-x.
    /// </summary>
    public int X { get; init; } = Int32.MinValue;

    /// <summary>
    /// На схеме это поле: Bounds-y.
    /// </summary>
    public int Y { get; init; } = Int32.MinValue;

    /// <summary>
    /// На схеме это поле: Bounds-width.
    /// </summary>
    public int Width { get; init; } = Int32.MinValue;

    /// <summary>
    /// На схеме это поле: Bounds-height.
    /// </summary>
    public int Height { get; init; } = Int32.MinValue;
}