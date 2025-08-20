using BpmnDotNet.Common.Models;
using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Elements.BpmnNatation;


public class StartEventComponent(string id, string[] outgoing) : IElement, IOutgoingPath
{
    public string IdElement { get; } = id;

    public ElementType ElementType { get; } = ElementType.StartEvent;

    public string[] Outgoing { get; } = outgoing;
}