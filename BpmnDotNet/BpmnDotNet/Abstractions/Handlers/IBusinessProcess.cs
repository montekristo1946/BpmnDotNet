namespace BpmnDotNet.Abstractions.Handlers;

using BpmnDotNet.Dto;

/// <summary>
/// BusinessProcess. Для внутреннего тестирования.
/// </summary>
internal interface IBusinessProcess
{
    /// <summary>
    ///     Gets получить статус процесса.
    /// </summary>
    public BusinessProcessJobStatus JobStatus { get; }

    /// <summary>
    ///     Добавить сообщение в очередь.
    /// </summary>
    /// <param name="messageType">Тип сообщения.</param>
    /// <param name="message">Сообщение.</param>
    /// <returns>Результат.</returns>
    public bool AddMessageToQueue(Type messageType, object message);
}