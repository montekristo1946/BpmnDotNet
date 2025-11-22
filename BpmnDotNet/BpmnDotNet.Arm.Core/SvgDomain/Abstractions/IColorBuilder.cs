namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Конструктор Color для SVG блоков.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface IColorBuilder<out T>
    where T : new()
{
    /// <summary>
    /// Добавить цвет.
    /// </summary>
    /// <param name="color">Цвет фигуры.</param>
    /// <returns>Вернет текущий блок.</returns>
    public T AddColor(string color);
}