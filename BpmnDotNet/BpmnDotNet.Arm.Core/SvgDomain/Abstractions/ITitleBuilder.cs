namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Конструктор Title для SVG блоков.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface ITitleBuilder<out T>
    where T : new()
{
    /// <summary>
    /// Добавить Титл.
    /// </summary>
    /// <param name="titleText">Текст титла.</param>
    /// <returns>Вернет текущий блок.</returns>
    public T AddTitle(string? titleText);
}