namespace BpmnDotNet.Common.Dto;

/// <summary>
/// Статусы процессов.
/// </summary>
public enum ProcessingStaus
{
    /// <summary>
    /// Не определенное состояние.
    /// </summary>
    None = 0,

    /// <summary>
    /// Ожидает запуска.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Сейчас работает.
    /// </summary>
    Works = 2,

    /// <summary>
    /// Удачно завершен.
    /// </summary>
    Complete = 3,

    /// <summary>
    /// Не удачно завершен.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Ожидает подтверждение выполненных путей (параллельный шлюз).
    /// </summary>
    WaitingCompletedWays = 5,

    /// <summary>
    /// Ожидает получения сообщения (для ReceivedTask)
    /// </summary>
    WaitingReceivedMessage = 6,
}