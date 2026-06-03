namespace BpmnDotNet.BpmnEngineDomain.Abstractions;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Движок выполнения Bpmn.
/// </summary>
internal interface IBpmnEngine
{
    /// <summary>
    /// Запустить процесс.
    /// </summary>
    /// <param name="contextBpmnProcess">Context.</param>
    /// <param name="processModel">ProcessModel.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>BusinessProcessJobStatus.</returns>
    public Task<BusinessProcessJobStatusV2> StartProcessAsync(
        IContextBpmnProcess contextBpmnProcess,
        ProcessModel processModel,
        CancellationToken ct);

    /// <summary>
    ///     Добавить сообщение в очередь.
    /// </summary>
    /// <param name="messageType">Тип сообщения.</param>
    /// <param name="message">Сообщение.</param>
    /// <returns>Результат.</returns>
    public bool AddMessageToQueue(Type messageType, object message);
}