using BpmnDotNet.Common.Models;

namespace BpmnDotNet.Abstractions.Elements;

/// <summary>
///     Базовый тип элемента.
/// </summary>
public interface IElement
{
    /// <summary>
    ///     Id.
    /// </summary>
    string IdElement { get; }

    /// <summary>
    ///     Тип Элемента.
    /// </summary>
    ElementType ElementType { get; }
}