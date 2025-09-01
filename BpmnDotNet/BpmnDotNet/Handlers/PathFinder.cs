using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Models;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Handlers;

public class PathFinder : IPathFinder
{
    private readonly ILogger<PathFinder> _logger;

    public PathFinder(ILogger<PathFinder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IElement[] GetStartEvent(IElement[]? elementsSrc)
    {
        ArgumentNullException.ThrowIfNull(elementsSrc);

        if (elementsSrc.Length == 0) throw new InvalidDataException(nameof(BpmnProcessDto));

        var res = elementsSrc.Where(p => p.ElementType == ElementType.StartEvent).ToArray();

        if (res.Length == 0)
            throw new InvalidDataException("[GetStartEvent] not find StartEvent");

        return res;
    }

    public IElement[] GetNextNode(IElement[]? elementsSrc, IElement[]? currentNodes, IContextBpmnProcess context)
    {
        ArgumentNullException.ThrowIfNull(elementsSrc);
        ArgumentNullException.ThrowIfNull(currentNodes);
        ArgumentNullException.ThrowIfNull(context);

        if (elementsSrc.Length == 0) throw new InvalidDataException(nameof(elementsSrc));

        if (currentNodes.Length == 0) throw new InvalidDataException(nameof(currentNodes));


        var elementsNode = new List<IElement>();

        foreach (var currentNode in currentNodes)
        {
            var elements = currentNode.ElementType switch
            {
                ElementType.StartEvent => OnPathGetNextNode(currentNode, elementsSrc),
                ElementType.ServiceTask => OnPathGetNextNode(currentNode, elementsSrc),
                ElementType.ExclusiveGateway => ExclusiveGatewayGetNextNode(currentNode, elementsSrc, context),
                ElementType.SendTask => OnPathGetNextNode(currentNode, elementsSrc),
                ElementType.ParallelGateway => ParallelGatewayGetNextNode(currentNode, elementsSrc),
                ElementType.EndEvent => EndEventGetNextNode(currentNode, elementsSrc),
                ElementType.ReceiveTask => OnPathGetNextNode(currentNode, elementsSrc),
                ElementType.SubProcess => OnPathGetNextNode(currentNode, elementsSrc),

                _ => throw new NotImplementedException($"Not find ImplementedException {currentNode.ElementType}")
            };

            elementsNode.AddRange(elements);
        }

        var retArr = ClearDuplicateNodes(elementsNode);

        return retArr;
    }

    //TODO: ошибка.
    public string GetConditionRouteWithExclusiveGateWay(IContextBpmnProcess context, IElement currentNode)
    {
        var outgoingNode = ElementOperator.GetOutgoingPath(currentNode);

        if (outgoingNode.Outgoing.Length == 1)
            return outgoingNode.Outgoing.First();

        if (context is not IExclusiveGateWay exclusiveGateWay)
            throw new InvalidDataException($"[GetConditionRouteWithExclusiveGateWay] " +
                                           $"The context does not implement IExclusiveGateWay but uses an exclusive gateway:{currentNode.IdElement}");

        var dict = exclusiveGateWay.ConditionRoute;

        if (!dict.TryGetValue(currentNode.IdElement, out var conditionName)
            || string.IsNullOrWhiteSpace(conditionName))
            throw new InvalidDataException($" [GetConditionRouteWithExclusiveGateWay] " +
                                           $"Couldn't find the condition from gateway:{currentNode.IdElement}");

        var checkPatch = outgoingNode.Outgoing.FirstOrDefault(p=>p == conditionName) ?? string.Empty;
            
        if(string.IsNullOrWhiteSpace(checkPatch))
            throw new InvalidDataException($" [GetConditionRouteWithExclusiveGateWay] " +
                                           $"There is no such way from gateway:{currentNode.IdElement}");
        
        return checkPatch;
    }

    private IEnumerable<IElement> EndEventGetNextNode(IElement currentNode, IElement[] elementsSrc)
    {
        return [];
    }

    private IElement[] ClearDuplicateNodes(IEnumerable<IElement> allElements)
    {
        var ret = allElements.DistinctBy(p => p.IdElement).ToArray();
        return ret;
    }

    private IElement[] ParallelGatewayGetNextNode(IElement currentNode, IElement[] elementsSrc)
    {
        var outgoingNode = ElementOperator.GetOutgoingPath(currentNode);

        var allFlow = outgoingNode.Outgoing.Select(flow =>
        {
            var elementFlow =
                elementsSrc.Where(p => p.IdElement == flow && p.ElementType == ElementType.SequenceFlow) ??
                throw new InvalidOperationException($"Not element type Flow, name: {flow}");
            return elementFlow;
        }).SelectMany(p => p).ToArray();

        var retArr = allFlow.Select(flow =>
        {
            var node = GetNextFromFlowElement(elementsSrc, flow);
            return node;
        }).ToArray();

        return retArr;
    }

    private IElement[] ExclusiveGatewayGetNextNode(IElement currentNode, IElement[] elementsSrc,
        IContextBpmnProcess context)
    {
        var conditionName = GetConditionRouteWithExclusiveGateWay(context, currentNode);

        var elementFlow =
            elementsSrc.FirstOrDefault(p =>
                p.IdElement == conditionName && p.ElementType == ElementType.SequenceFlow) ??
            throw new InvalidOperationException($"Not element type Flow, name: {conditionName}");

        var elementNext = GetNextFromFlowElement(elementsSrc, elementFlow);

        return [elementNext];
    }


    private IElement[] OnPathGetNextNode(IElement currentNode, IElement[] elementsSrc)
    {
        var outgoingNodes = ElementOperator.GetOutgoingPath(currentNode);

        if (outgoingNodes.Outgoing.Length != 1)
            throw new InvalidOperationException(
                $"There can be no outputs from the {currentNode.ElementType} block != 1; curren {outgoingNodes.Outgoing.Length}");

        var outgoingIdStartEvent = outgoingNodes.Outgoing.First();

        var elementFlow = elementsSrc.FirstOrDefault(p =>
                              p.IdElement == outgoingIdStartEvent && p.ElementType == ElementType.SequenceFlow) ??
                          throw new InvalidOperationException($"Not element type Flow, name: {outgoingIdStartEvent}");

        var elementNext = GetNextFromFlowElement(elementsSrc, elementFlow);

        return [elementNext];
    }

    private IElement GetNextFromFlowElement(IElement[] elementsSrc, IElement elementFlow)
    {
        var outgoingFlow = ElementOperator.GetOutgoingPath(elementFlow);

        if (outgoingFlow.Outgoing.Length != 1)
            throw new InvalidOperationException(
                $"There can be no outputs from the Flow  != 1; current {outgoingFlow.Outgoing.Length}");

        var outgoingIdFlow = outgoingFlow.Outgoing.First();

        var elementNext = elementsSrc.FirstOrDefault(p => p.IdElement == outgoingIdFlow) ??
                          throw new InvalidOperationException(
                              $"Not found Outgoing element from Flow, name: {elementFlow.IdElement}");
        return elementNext;
    }
}