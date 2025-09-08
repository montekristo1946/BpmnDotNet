namespace BpmnDotNet.Abstractions.Handlers;

using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Dto;

/// <summary>
/// Реализация клиента.
/// </summary>
public interface IBpmnClient : IDisposable
{
    /// <summary>
    ///     Запуск процесса.
    /// </summary>
    /// <param name="context">Контекст операции.</param>
    /// <param name="timeout">Выделенное время на весь процесс.</param>
    /// <returns>BusinessProcessJobStatus.</returns>
    public BusinessProcessJobStatus StartNewProcess(IContextBpmnProcess context, TimeSpan timeout);

    /// <summary>
    ///     Регистрация handler.
    /// </summary>
    /// <param name="handlerBpmn">Хэндлер.</param>
    /// <typeparam name="THandler">Тип хэндлера.</typeparam>
    public void RegisterHandlers<THandler>(THandler handlerBpmn)
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