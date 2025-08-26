using BpmnDotNet.Common.Models;
using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Elements.BpmnNatation;

public class SequenceFlowComponent(string id, string[] incoming, string[] outgoing)
    : IElement, IIncomingPath, IOutgoingPath
{
    public string IdElement { get; } = id;

    public ElementType ElementType { get; } = ElementType.SequenceFlow;

    public string[] Incoming { get; } = incoming;

    public string[] Outgoing { get; } = outgoing;
}