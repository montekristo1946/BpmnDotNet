using System.Collections.Concurrent;
using BpmnDotNet.Common.Abstractions;
using Sample.ConsoleApp.Common;

namespace Sample.ConsoleApp.Context;

public class ContextData : IContextBpmnProcess, IExclusiveGateWayRoute, IMessageReceiveTask
{
    public int TestValue { get; set; } = 0;

    public string TestValue2 { get; set; } = string.Empty;
    public string IdBpmnProcess { get; init; } = Constants.IdBpmnProcessingMain;

    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();

    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();

    public ConcurrentDictionary<string, Type> RegistrationMessagesType { get; init; } = new();

    public ConcurrentDictionary<Type, object> ReceivedMessage { get; init; } = new();
}