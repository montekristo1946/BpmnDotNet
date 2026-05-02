using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using Sample.ConsoleApp.Common;

namespace Sample.ConsoleApp.Context;

internal class ContextSubProcess : IContextBpmnProcess
{
    public string ContextSubProcessValue { get; set; } = string.Empty;
    public string IdBpmnProcess { get; init; } = Constants.IdBpmnProcessingSub;
    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();
    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();
    public ConcurrentDictionary<string, Type> RegistrationMessagesType { get; init; } = new();
    public ConcurrentDictionary<Type, object> ReceivedMessage { get; init; } = new();
}