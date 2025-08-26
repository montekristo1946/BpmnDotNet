using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Dto;

namespace BpmnDotNet.Interfaces.Handlers;

public interface IBpmnClient : IDisposable
{
    /// <summary>
    ///     Запуск процесса.
    /// </summary>
    /// <param name="context">Контекст операции.</param>
    /// <param name="timeout">Выделенное время на весь процесс</param>
    BusinessProcessJobStatus StartNewProcess(IContextBpmnProcess context, TimeSpan timeout);

    /// <summary>
    ///     Регистрация handler.
    /// </summary>
    /// <param name="handlerBpmn"></param>
    /// <typeparam name="THandler"></typeparam>
    void RegisterHandlers<THandler>(THandler handlerBpmn) where THandler : IBpmnHandler;

    /// <summary>
    ///     Отправка сообщения для блоков received Task
    /// </summary>
    /// <param name="idBpmnProcess"></param>
    /// <param name="tokenProcess"></param>
    /// <param name="messageType"></param>
    /// <param name="message"></param>
    void SendMessage(string idBpmnProcess, string tokenProcess, Type messageType, object message);
}