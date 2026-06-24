namespace BpmnDotNet.BPMNDiagram.BpmnNatation;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Node в объектовом виде, EndEvent.
/// </summary>
/// <param name="id">Id.</param>
internal class EndEventComponent(string id) : IElement
{
    /// <summary>
    /// Gets id.
    /// </summary>
    public string IdElement { get; } = id;
}