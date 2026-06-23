namespace BpmnDotNet.Abstractions.Elements;

using BpmnDotNet.BPMNDiagram;

/// <summary>
///     Базовый тип элемента.
/// </summary>
public interface IElement
{
    /// <summary>
    ///     Gets id.
    /// </summary>
    string IdElement { get; }
}