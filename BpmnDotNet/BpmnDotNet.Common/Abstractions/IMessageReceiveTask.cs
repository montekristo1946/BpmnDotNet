namespace BpmnDotNet.Common.Abstractions;

using System;
using System.Collections.Concurrent;

/// <summary>
///     Хранилище зарегистрированных сообщений.
/// </summary>
public interface IMessageReceiveTask
{
    /// <summary>
    ///     Gets хранилище соответствий ReceiveTask -> сообщений. Где: string - id node ReceiveTask, второй Type
    ///     сообщения.
    /// </summary>
    public ConcurrentDictionary<string, Type> RegistrationMessagesType { get; init; }

    /// <summary>
    ///     Gets полученные сообщения. Где string - typeName, object - само Dto сообщения.
    /// </summary>
    public ConcurrentDictionary<Type, object> ReceivedMessage { get; init; }
}