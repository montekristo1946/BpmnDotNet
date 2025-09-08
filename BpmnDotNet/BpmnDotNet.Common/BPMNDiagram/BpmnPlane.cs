namespace BpmnDotNet.Common.BPMNDiagram;

using System;

/// <summary>
///     В нотации секция: bpmndi:BPMNPlane.
/// </summary>
public record BpmnPlane
{
    /// <summary>
    ///     Gets на схеме это поле:id.
    /// </summary>
    public string Id => IdBpmnProcess;

    /// <summary>
    ///     Gets на схеме это поле: bpmnElement (Id всего процесса).
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Gets массив фигур для отрисовки.
    /// </summary>
    public BpmnShape[] Shapes { get; init; } = [];
}