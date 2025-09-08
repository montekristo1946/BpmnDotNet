namespace BpmnDotNet.Elements.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Common.Models;

/// <summary>
/// Node в объектовом виде, ReceiveTask.
/// </summary>
/// <param name="id">ID.</param>
/// <param name="incoming">Входные Flow.</param>
/// <param name="outgoing">Выходные Flow.</param>
public class ReceiveTaskComponent(string id, string[] incoming, string[] outgoing)
    : IElement, IIncomingPath, IOutgoingPath
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;

    /// <summary>
    /// Gets elementType.
    /// </summary>
    public ElementType ElementType { get; } = ElementType.ReceiveTask;

    /// <summary>
    /// Gets входные Flow.
    /// </summary>
    public string[] Incoming { get; } = incoming;

    /// <summary>
    /// Gets выходные Flow.
    /// </summary>
    public string[] Outgoing { get; } = outgoing;
}