namespace BpmnDotNet.BPMNDiagram.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Node в объектовом виде, ServiceTask.
/// </summary>
/// <param name="id">ID.</param>
public class ServiceTaskComponent(string id) : IElement
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;
}