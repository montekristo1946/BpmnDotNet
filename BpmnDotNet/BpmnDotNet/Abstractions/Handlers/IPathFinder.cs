using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Common.Abstractions;

namespace BpmnDotNet.Abstractions.Handlers;

public interface IPathFinder
{
    IElement[] GetStartEvent(IElement[]? elementsSrc);

    public IElement[] GetNextNode(IElement[]? elementsSrc, IElement[]? currentNodes, IContextBpmnProcess context);

    public string GetConditionRouteWithExclusiveGateWay(IContextBpmnProcess context, IElement currentNode);
}