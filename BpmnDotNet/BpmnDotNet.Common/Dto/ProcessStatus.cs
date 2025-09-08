namespace BpmnDotNet.Common.Dto;

/// <summary>
///     Статусы процессов.
/// </summary>
public enum ProcessStatus
{
    /// <summary>
    ///     Не определенное состояние.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Сейчас работает.
    /// </summary>
    Works = 1,

    /// <summary>
    ///     Удачно завершен.
    /// </summary>
    Completed = 2,

    /// <summary>
    ///     Не удачно завершен.
    /// </summary>
    Error = 3,
}