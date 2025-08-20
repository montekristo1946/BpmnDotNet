using System.Diagnostics;
using BpmnDotNet.Common.Models;
using BpmnDotNet.Elements;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;

namespace BpmnDotNet.Handlers;

internal class CheckBpmnProcessDto : ICheckBpmnProcessDto
{
    public void Check(BpmnProcessDto bpmnProcess)
    {
        ArgumentNullException.ThrowIfNull(bpmnProcess);
        ArgumentNullException.ThrowIfNull(bpmnProcess.ElementsFromBody);

        var elements = bpmnProcess.ElementsFromBody;

        if (elements.Any() is false)
            throw new ArgumentNullException(nameof(elements));


        CheckAvailabilityOutgoings(elements,bpmnProcess.IdBpmnProcess);
        CheckAvailabilityIncomingPath(elements, bpmnProcess.IdBpmnProcess);
        CheckBeginningAndEnd(elements, bpmnProcess.IdBpmnProcess);
        CheckCountGetaway(elements, bpmnProcess.IdBpmnProcess);
    }

    private void CheckCountGetaway(IElement[] elements, string bpmnProcessIdBpmnProcess)
    {
        var exclusiveGateway = elements.Where(p => p.ElementType == ElementType.ExclusiveGateway).ToArray();
        if (exclusiveGateway.Length % 2 != 0)
        {
            throw new InvalidDataException($"{bpmnProcessIdBpmnProcess} Invalid count of elements ExclusiveGateway");
        }
            
        var parallelGateway = elements.Where(p => p.ElementType == ElementType.ParallelGateway).ToArray();
        if (parallelGateway.Length % 2 != 0)
        {
            throw new InvalidDataException($"{bpmnProcessIdBpmnProcess} Invalid count of elements ParallelGateway");
        }
        
    }

    private void CheckBeginningAndEnd(IElement[] elements, string bpmnProcessIdBpmnProcess)
    {
        var startEvent = elements.Where(p => p.ElementType == ElementType.StartEvent).ToArray();
        if(startEvent.Length != 1)
            throw new InvalidDataException($"{bpmnProcessIdBpmnProcess} Invalid count of elements start event");

        var endEvent = elements.Where(p => p.ElementType == ElementType.EndEvent).ToArray();
        if(endEvent.Length != 1)
            throw new InvalidDataException($"{bpmnProcessIdBpmnProcess} Invalid count of elements end event");
        
    }

    private void CheckAvailabilityIncomingPath(IElement[] elements, string bpmnProcessIdBpmnProcess)
    {
        foreach (var element in elements)
        {
            switch (element.ElementType)
            {
                case ElementType.EndEvent:
                case ElementType.SequenceFlow:
                case ElementType.SendTask:
                case ElementType.ServiceTask:
                case ElementType.ReceiveTask:
                case ElementType.SubProcess:
                    CheckIncomingPathOneWay(element,bpmnProcessIdBpmnProcess);
                    break;
                
                case ElementType.StartEvent:
                case ElementType.ExclusiveGateway:
                case ElementType.ParallelGateway:
                    break;

              
                default:
                    throw new ArgumentOutOfRangeException(element.ElementType.ToString());
            }
        }
    }

    private void CheckIncomingPathOneWay(IElement element, string bpmnProcessIdBpmnProcess)
    {
        var res = ElementOperator.GetIncomingPath(element);
        if (res.Incoming.Length != 1)
            throw new InvalidDataException(
                $"{bpmnProcessIdBpmnProcess} Outgoing elements must have exactly one incoming element. {element.IdElement}");
    }

    private void CheckAvailabilityOutgoings(IElement[] elements, string bpmnProcessIdBpmnProcess)
    {
        foreach (var element in elements)
        {
            switch (element.ElementType)
            {
                case ElementType.StartEvent:
                case ElementType.SequenceFlow:
                case ElementType.SendTask:
                case ElementType.ServiceTask:
                case ElementType.ReceiveTask:
                case ElementType.SubProcess:
                    CheckOutgoingsOneWay(element,bpmnProcessIdBpmnProcess);
                    break;
                
                case ElementType.EndEvent:
                case ElementType.ExclusiveGateway:
                case ElementType.ParallelGateway:
                    break;

              
                default:
                    throw new ArgumentOutOfRangeException(element.ElementType.ToString());
            }
        }
    }

    private void CheckOutgoingsOneWay(IElement element, string bpmnProcessIdBpmnProcess)
    {
        var res = ElementOperator.GetOutgoingPath(element);
        if (res.Outgoing.Length != 1)
            throw new InvalidDataException(
                $"{bpmnProcessIdBpmnProcess} Outgoing elements must have exactly one outgoing element. {element.IdElement}");
    }
}