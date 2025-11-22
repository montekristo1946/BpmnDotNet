namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Реализация SVG блока.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface IBpmnBuild<out T>
    where T : new()
{
    /// <summary>
    /// Построить готовый блок.
    /// </summary>
    /// <returns>SVG фигуры.</returns>
    public string BuildSvg();

    /// <summary>
    /// Создать инстанс.
    /// </summary>
    /// <returns>Тип блока.</returns>
    public static T Create()
    {
        return new T();
    }

    /// <summary>
    /// Добавить дочерний элемент.
    /// </summary>
    /// <param name="childElement">Реализация дочернего элемента.</param>
    /// <returns>Вернет текущий экземпляр.</returns>
    public T AddChild(string childElement);

    /// <summary>
    /// Добавить ID.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <returns>Обьект сборки.</returns>
    public T AddId(string id);
}