namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

using BpmnDotNet.BPMNDiagram;

/// <summary>
/// Конструктор Tspan для SVG блоков.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface ITspan<out T>
    where T : new()
{
    /// <summary>
    /// Смещение по оси Y.
    /// </summary>
    /// <param name="value">value.</param>
    /// <returns>Объект создания.</returns>
    public T AddPaddingY(int value);

    /// <summary>
    /// Размер блока по ширине, в px.
    /// </summary>
    /// <param name="value">value.</param>
    /// <returns>Объект создания.</returns>
    public T AddBoundBlock(Bound value);
}