using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Handlers;

internal static class ElementOperator
{
    
    public static IOutgoingPath GetOutgoingPath(IElement currentNode)
    {
        var outgoingPath = (IOutgoingPath)currentNode;

        if (outgoingPath.Outgoing is null || outgoingPath.Outgoing.Length == 0)
            throw new InvalidDataException($"{currentNode.ElementType} does not contain Outgoing: {currentNode.IdElement}");

        return outgoingPath;
    }
    
    public static IIncomingPath GetIncomingPath(IElement currentNode)
    {
        var outgoingPath = (IIncomingPath)currentNode;

        if (outgoingPath.Incoming is null || outgoingPath.Incoming.Length == 0)
            throw new InvalidDataException($"{currentNode.ElementType} does not contain IncomingPath: {currentNode.IdElement}");

        return outgoingPath;
    }
    
    
}