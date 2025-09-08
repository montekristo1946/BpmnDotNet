namespace BpmnDotNet.Abstractions.Elements;

using BpmnDotNet.Common.Models;

/// <summary>
///     Базовый тип элемента.
/// </summary>
public interface IElement
{
    /// <summary>
    ///     Gets id.
    /// </summary>
    string IdElement { get; }

    /// <summary>
    ///     Gets тип Элемента.
    /// </summary>
    ElementType ElementType { get; }
}