using BpmnDotNet.Dto;
using BpmnDotNet.Handlers;

namespace BpmnDotNet.Interfaces.Handlers;

internal interface IBusinessProcess
{
    // /// <summary>
    // /// Запустить процесс
    // /// </summary>
    // /// <param name="ctsToken"></param>
    // /// <returns></returns>
    // Task<BusinessProcessJobStatus> StartBusinessProcess(CancellationToken ctsToken);

    /// <summary>
    /// Добавить сообщение в очередь.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="message"></param>
    bool AddMessageToQueue(Type messageType, object message);

    /// <summary>
    /// Получить статус процесса.
    /// </summary>
    public BusinessProcessJobStatus JobStatus { get; }

}