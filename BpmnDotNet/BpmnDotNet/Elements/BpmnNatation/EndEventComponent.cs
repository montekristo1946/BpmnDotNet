using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Elements.BpmnNatation;

public class EndEventComponent(string id, string[] incoming) : IElement, IIncomingPath
{
    public string IdElement { get; } = id;

    public ElementType ElementType { get; } = ElementType.EndEvent;

    public string[] Incoming { get; } = incoming;
}