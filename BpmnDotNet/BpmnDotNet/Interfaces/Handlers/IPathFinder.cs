using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Interfaces.Handlers;

public interface IPathFinder
{
    IElement[] GetStartEvent(IElement[]? elementsSrc);

    public IElement[] GetNextNode(IElement[]? elementsSrc, IElement[]? currentNodes, IContextBpmnProcess context);

    public string GetConditionRouteWithExclusiveGateWay(IContextBpmnProcess context, IElement currentNode);
}