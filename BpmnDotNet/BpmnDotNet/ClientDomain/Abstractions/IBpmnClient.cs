namespace BpmnDotNet.ClientDomain.Abstractions;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Реализация клиента.
/// </summary>
public interface IBpmnClient : IAsyncDisposable
{
    /// <summary>
    ///     Запуск процесса.
    /// </summary>
    /// <param name="context"><see cref="IContextBpmnProcess"/>.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns><see cref="BusinessProcessJobStatus"/>.</returns>
    public Task<BusinessProcessJobStatus> StartNewProcessAsync(IContextBpmnProcess context, CancellationToken cancellationToken);

    /// <summary>
    ///     Регистрация handlers.
    /// </summary>
    /// <param name="handlerBpmn">Хэндлер.</param>
    /// <typeparam name="THandler">Тип хэндлера.</typeparam>
    public void RegisterHandlers<THandler>(THandler[] handlerBpmn)
        where THandler : IBpmnHandler;

    /// <summary>
    ///     Отправка сообщения для блоков received Task.
    /// </summary>
    /// <param name="idBpmnProcess">idBpmnProcess.</param>
    /// <param name="tokenProcess">tokenProcess.</param>
    /// <param name="messageType">Тип сообщения.</param>
    /// <param name="message">Сообщение.</param>
    public void SendMessage(string idBpmnProcess, string tokenProcess, Type messageType, object message);
}