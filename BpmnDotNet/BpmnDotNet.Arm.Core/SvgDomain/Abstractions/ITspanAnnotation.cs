namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Конструктор Tspan Annotation для SVG блоков.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface ITspanAnnotation<out T>
    where T : new()
{
    /// <summary>
    /// Размер блока по ширине, в px.
    /// </summary>
    /// <param name="value">value.</param>
    /// <returns>Объект создания.</returns>
    public T AddWidthBlock(int value);
}