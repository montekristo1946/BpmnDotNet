namespace BpmnDotNet.Elements.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Common.Models;

/// <summary>
/// Node в объектовом виде, StartEvent.
/// </summary>
/// <param name="id">ID.</param>
/// <param name="outgoing">Выходные Flow.</param>
public class StartEventComponent(string id, string[] outgoing) : IElement, IOutgoingPath
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;

    /// <summary>
    /// Gets elementType.
    /// </summary>
    public ElementType ElementType { get; } = ElementType.StartEvent;

    /// <summary>
    /// Gets выходные Flow.
    /// </summary>
    public string[] Outgoing { get; } = outgoing;
}