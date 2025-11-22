namespace BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
/// Конструктор смещения позиции на svg.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface IOffsetsPosition<out T>
    where T : new()
{
    /// <summary>
    /// Добавить смещение.
    /// </summary>
    /// <param name="x">Координата Х.</param>
    /// <param name="y">Координата У.</param>
    /// <returns>Текущий блок.</returns>
    public T AddPositionOffsets(int x, int y);
}