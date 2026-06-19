namespace BpmnDotNet.BPMNDiagram.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Node в объектовом виде, ExclusiveGateway.
/// </summary>
/// <param name="id">ID.</param>
public class SubProcessComponent(string id) : IElement
{
    /// <summary>
    /// Gets iD.
    /// </summary>
    public string IdElement { get; } = id;

    /// <summary>
    /// Gets elementType.
    /// </summary>
    public ElementType ElementType { get; } = ElementType.SubProcess;
}