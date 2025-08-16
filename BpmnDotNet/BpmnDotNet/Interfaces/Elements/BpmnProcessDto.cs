namespace BpmnDotNet.Interfaces.Elements;

public class BpmnProcessDto(string idBpmnProcess, IElement[] elementsFromBody)
{
    public string IdBpmnProcess { get; init; } = idBpmnProcess;

    public IElement[] ElementsFromBody { get; init; } = elementsFromBody;
}