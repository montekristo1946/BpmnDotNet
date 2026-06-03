namespace BpmnDotNet.BPMNDiagram.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Node в объектовом виде, SendTask.
/// </summary>
/// <param name="id">ID.</param>
public class SendTaskComponent(string id) : IElement
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;

    /// <summary>
    /// Gets elementType.
    /// </summary>
    public ElementType ElementType { get; } = ElementType.SendTask;
}