namespace BpmnDotNet.BPMNDiagram.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Node в объектовом виде, ReceiveTask.
/// </summary>
/// <param name="id">ID.</param> >
public class ReceiveTaskComponent(string id) : IElement
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;
}