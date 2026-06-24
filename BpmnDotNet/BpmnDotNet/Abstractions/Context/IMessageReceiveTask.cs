namespace BpmnDotNet.Abstractions.Context;

using System.Collections.Concurrent;

/// <summary>
///     Хранилище зарегистрированных сообщений.
/// </summary>
public interface IMessageReceiveTask
{
    /// <summary>
    ///     Gets хранилище соответствий ReceiveTask -> сообщений. Где:  Type message, string - id node ReceiveTask
    ///     сообщения.
    /// </summary>
    public ConcurrentDictionary<Type, string> RegistrationMessagesType { get; init; }

    /// <summary>
    ///     Gets полученные сообщения. Где string - id node, object - само Dto сообщения.
    /// </summary>
    public ConcurrentDictionary<string, object> ReceivedMessage { get; init; }
}