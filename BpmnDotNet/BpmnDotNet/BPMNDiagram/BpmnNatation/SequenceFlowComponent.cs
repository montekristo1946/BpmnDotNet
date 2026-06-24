namespace BpmnDotNet.BPMNDiagram.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Node в объектовом виде, SequenceFlow.
/// </summary>
/// <param name="id">ID.</param>
/// <param name="sourceId">Входные Flow.</param>
/// <param name="targetId">Выходные Flow.</param>
public class SequenceFlowComponent(string id, string sourceId, string targetId)
    : IElement, IIncomingPath, IOutgoingPath
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;

    /// <summary>
    /// Gets входные Flow.
    /// </summary>
    public string SourceId { get; } = sourceId;

    /// <summary>
    /// Gets выходные Flow.
    /// </summary>
    public string TargetId { get; } = targetId;
}