using System.Collections.Concurrent;

namespace BpmnDotNet.Interfaces.Elements;

/// <summary>
/// Хранилище зарегистрированных сообщений.
/// </summary>
public interface IMessageReceiveTask
{
    /// <summary>
    /// Хранилище соответствий ReceiveTask -> сообщений. Где: string - id node ReceiveTask, второй string typeName  сообщения.
    /// </summary>
    public ConcurrentDictionary<string, Type> RegistrationMessagesType { get; init; }

    /// <summary>
    /// Полученные сообщения. Где string - typeName, object - само Dto сообщения
    /// </summary>
    public ConcurrentDictionary<Type, object> ReceivedMessage { get; init; }
}