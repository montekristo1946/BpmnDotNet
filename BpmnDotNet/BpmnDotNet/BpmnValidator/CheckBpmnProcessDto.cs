namespace BpmnDotNet.BpmnValidator;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.BPMNDiagram.BpmnNatation;
using BpmnDotNet.BpmnValidator.Abstractions;
using BpmnDotNet.Dto;

/// <inheritdoc />
internal class CheckBpmnProcessDto : ICheckBpmnProcessDto
{
    /// <inheritdoc/>
    public void Check(BpmnProcessDto bpmnProcess)
    {
        var elementsFromBody = bpmnProcess.ElementsFromBody;
        var idBpmn = bpmnProcess.IdBpmnProcess;
        HasStartAndEndEvents(elementsFromBody, idBpmn);
        HasOneTarget(elementsFromBody, idBpmn);
        HasStartAndEndEvents(elementsFromBody, idBpmn);
        HasOneStartEven(elementsFromBody, idBpmn);
        HasNotOneTargetExclusiveGateway(elementsFromBody, idBpmn);
    }

    /// <summary>
    /// Элементы с одним выходом.
    /// </summary>
    /// <param name="elements"><see cref="IElement"/>.</param>
    /// <param name="idBpmn">id bpmn.</param>
    /// <exception cref="NotImplementedException"><see cref="NotImplementedException"/>.</exception>
    internal void HasOneTarget(IElement[] elements, string idBpmn)
    {
        var flows = elements.OfType<SequenceFlowComponent>().ToArray();
        foreach (var element in elements)
        {
            var result = element switch
            {
                StartEventComponent startEvent => CheckTargetPathOneWay(startEvent.IdElement, flows),
                ReceiveTaskComponent endEvent => CheckTargetPathOneWay(endEvent.IdElement, flows),
                SendTaskComponent endEvent => CheckTargetPathOneWay(endEvent.IdElement, flows),
                ServiceTaskComponent endEvent => CheckTargetPathOneWay(endEvent.IdElement, flows),
                SubProcessComponent endEvent => CheckTargetPathOneWay(endEvent.IdElement, flows),

                ParallelGatewayComponent => true,
                ExclusiveGatewayComponent => true,
                SequenceFlowComponent => true,
                EndEventComponent => true,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(elements),
                    $"[CheckBpmnProcessDto:HasOneTarget] Unsupported element type: {element.GetType().Name}"),
            };
            if (!result)
            {
                throw new InvalidDataException(
                    $"[CheckBpmnProcessDto:HasOneTarget] {idBpmn} Outgoing elements must have exactly one target element. {element.IdElement}");
            }
        }
    }

    /// <summary>
    /// Проверить что на схеме есть start even и  endEvent.
    /// </summary>
    /// <param name="elements"><see cref="IElement"/>.</param>
    /// <param name="idBpmn">id bpmn.</param>
    /// <exception cref="NotImplementedException"><see cref="NotImplementedException"/>.</exception>
    internal virtual void HasStartAndEndEvents(IElement[] elements, string idBpmn)
    {
        var start = elements.OfType<StartEventComponent>().FirstOrDefault();
        if (start == null)
        {
            throw new InvalidDataException($"Not StartEvent element found {typeof(StartEventComponent)}: {idBpmn}");
        }

        var endEvent = elements.OfType<EndEventComponent>().FirstOrDefault();
        if (endEvent == null)
        {
            throw new InvalidDataException($"Not EndEvent element found {typeof(EndEventComponent)}: {idBpmn}");
        }
    }

    /// <summary>
    /// На схеме должен быть один startEvent.
    /// </summary>
    /// <param name="elements"><see cref="IElement"/>.</param>
    /// <param name="idBpmn">id bpmn.</param>
    /// <exception cref="NotImplementedException"><see cref="NotImplementedException"/>.</exception>
    internal void HasOneStartEven(IElement[] elements, string idBpmn)
    {
        const int countValide = 1;
        var count = elements.OfType<StartEventComponent>().Count();
        if (count != countValide)
        {
            throw new InvalidDataException($"There should be only one StartEvent on the diagram, find: {count}: {idBpmn}");
        }
    }

    /// <summary>
    /// ExclusiveGateway не может быть один выход.
    /// </summary>
    /// <param name="elements"><see cref="IElement"/>.</param>
    /// <param name="idBpmn">id bpmn.</param>
    /// <exception cref="NotImplementedException"><see cref="NotImplementedException"/>.</exception>
    internal void HasNotOneTargetExclusiveGateway(IElement[] elements, string idBpmn)
    {
        const int countValide = 2;
        var gateways = elements.OfType<ExclusiveGatewayComponent>().ToArray();
        var flows = elements.OfType<SequenceFlowComponent>().ToArray();
        foreach (var getaway in gateways)
        {
            var countSource = flows.Count(p => p.SourceId == getaway.IdElement);
            if (countSource < countValide)
            {
                throw new InvalidDataException(
                    $"ExclusiveGateway cannot have less than two outputs, find: {countSource}:{getaway.IdElement}: {idBpmn}");
            }
        }
    }

    private bool CheckTargetPathOneWay(string id, SequenceFlowComponent[] flows)
    {
        var countSource = flows.Count(p => p.SourceId == id);

        const int countValide = 1;

        return countSource == countValide;
    }
}