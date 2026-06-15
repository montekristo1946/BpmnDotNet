namespace BpmnDotNet.BpmnEngineDomain.Dto;

//TODO: Видимо не пригодилось.
/// <summary>
/// Состояния BpmnEngine.
/// </summary>
public enum StatusBpmnEngine
{
    /// <summary>
    ///     Не определенное состояние.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Сейчас выполняется.
    /// </summary>
    Works = 1,

    /// <summary>
    ///     Удачно завершен.
    /// </summary>
    Completed = 2,

    /// <summary>
    ///     Не удачно завершен.
    /// </summary>
    Failed = 3,
}