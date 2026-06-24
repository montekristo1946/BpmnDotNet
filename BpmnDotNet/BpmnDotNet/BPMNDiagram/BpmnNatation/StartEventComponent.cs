namespace BpmnDotNet.BPMNDiagram.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Node в объектовом виде, StartEvent.
/// </summary>
/// <param name="id">ID.</param>
public class StartEventComponent(string id) : IElement
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;
}