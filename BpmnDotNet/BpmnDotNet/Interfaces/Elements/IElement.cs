using BpmnDotNet.Common.Models;
using BpmnDotNet.Elements;

namespace BpmnDotNet.Interfaces.Elements;

/// <summary>
/// Базовый тип элемента.
/// </summary>
public interface IElement
{
    /// <summary>
    /// Id.
    /// </summary>
    string IdElement { get; }

    /// <summary>
    /// Тип Элемента.
    /// </summary>
    ElementType ElementType { get; }
}