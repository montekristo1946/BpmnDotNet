namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

using BpmnDotNet.BPMNDiagram;

/// <summary>
/// Конструктор bounds элемента.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface IBoundsPosition<out T>
    where T : new()
{
    /// <summary>
    /// Добавить координаты прямоугольника.
    /// </summary>
    /// <param name="bound">Координата прямоугольника.</param>
    /// <returns>Текущий блок.</returns>
    public T AddBounds(Bound bound);
}