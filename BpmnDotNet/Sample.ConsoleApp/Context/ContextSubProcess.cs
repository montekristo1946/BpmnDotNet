using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using Sample.ConsoleApp.Common;

namespace Sample.ConsoleApp.Context;

internal class ContextSubProcess : IContextBpmnProcess
{
    /// <summary>
    /// Кастомное поле, для примера.
    /// </summary>
    public string ContextSubProcessValue { get; set; } = string.Empty;

    /// <inheritdoc />
    public string IdBpmnProcess { get; init; } = Constants.IdBpmnProcessingSub;

    /// <inheritdoc />
    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();

    /// <inheritdoc />
    public ConcurrentDictionary<Type, string> RegistrationMessagesType { get; init; } = new();

    /// <inheritdoc />
    public ConcurrentDictionary<string, object> ReceivedMessage { get; init; } = new();
}