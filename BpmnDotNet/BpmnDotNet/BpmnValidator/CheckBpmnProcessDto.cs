using BpmnDotNet.BPMNDiagram.BpmnNatation;

namespace BpmnDotNet.BpmnValidator;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.BpmnEngineDomain.Activity;
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
}