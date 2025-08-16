using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Elements.BpmnNatation;

public class ExclusiveGatewayComponent(string id, string[] incoming, string[] outgoing) : IElement, IIncomingPath, IOutgoingPath
{
    public string IdElement { get; } = id;

    public ElementType ElementType { get; } = ElementType.ExclusiveGateway;

    public string[] Incoming { get; } = incoming;

    public string[] Outgoing { get; } = outgoing;
}