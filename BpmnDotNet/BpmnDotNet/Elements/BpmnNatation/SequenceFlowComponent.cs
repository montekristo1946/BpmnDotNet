namespace BpmnDotNet.Elements.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Common.Models;

/// <summary>
/// Node в объектовом виде, SequenceFlow.
/// </summary>
/// <param name="id">ID.</param>
/// <param name="incoming">Входные Flow.</param>
/// <param name="outgoing">Выходные Flow.</param>
public class SequenceFlowComponent(string id, string[] incoming, string[] outgoing)
    : IElement, IIncomingPath, IOutgoingPath
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;

    /// <summary>
    /// Gets elementType.
    /// </summary>
    public ElementType ElementType { get; } = ElementType.SequenceFlow;

    /// <summary>
    /// Gets входные Flow.
    /// </summary>
    public string[] Incoming { get; } = incoming;

    /// <summary>
    /// Gets выходные Flow.
    /// </summary>
    public string[] Outgoing { get; } = outgoing;
}