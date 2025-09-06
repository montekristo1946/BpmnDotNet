using BpmnDotNet.Dto;

namespace BpmnDotNet.Abstractions.Handlers;

internal interface IBusinessProcess
{
    /// <summary>
    ///     Получить статус процесса.
    /// </summary>
    public BusinessProcessJobStatus JobStatus { get; }
    // /// <summary>
    // /// Запустить процесс
    // /// </summary>
    // /// <param name="ctsToken"></param>
    // /// <returns></returns>
    // Task<BusinessProcessJobStatus> StartBusinessProcess(CancellationToken ctsToken);

    /// <summary>
    ///     Добавить сообщение в очередь.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="message"></param>
    bool AddMessageToQueue(Type messageType, object message);
}