namespace BpmnDotNet.BpmnEngineDomain.Abstractions;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Движок выполнения Bpmn.
/// </summary>
internal interface IBpmnEngine : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether процесс завершен.
    /// </summary>
    public bool IsProcessCancel { get; }

    /// <summary>
    /// Запустить процесс.
    /// </summary>
    /// <param name="contextBpmnProcess">Context.</param>
    /// <param name="processModel">ProcessModel.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>BusinessProcessJobStatus.</returns>
    public Task<BusinessProcessJobStatus> StartProcessAsync(
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