namespace BpmnDotNet.BPMNDiagram;

/// <summary>
///     В нотации секция: di:waypoint.
/// </summary>
public record Waypoint
{
    /// <summary>
    ///     Gets на схеме это поле: Bounds-x.
    /// </summary>
    public int X { get; init; } = int.MinValue;

    /// <summary>
    ///     Gets на схеме это поле: Bounds-y.
    /// </summary>
    public int Y { get; init; } = int.MinValue;
}