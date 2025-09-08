namespace BpmnDotNet.Elements.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Common.Models;

/// <summary>
/// Node в объектовом виде, EndEvent.
/// </summary>
/// <param name="id">Id.</param>
/// <param name="incoming">Входные Flow.</param>
internal class EndEventComponent(string id, string[] incoming) : IElement, IIncomingPath
{
    /// <summary>
    /// Gets id.
    /// </summary>
    public string IdElement { get; } = id;

    /// <summary>
    /// Gets elementType.
    /// </summary>
    public ElementType ElementType { get; } = ElementType.EndEvent;

    /// <summary>
    /// Gets входные Flow.
    /// </summary>
    public string[] Incoming { get; } = incoming;
}