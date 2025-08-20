using System.Collections.Concurrent;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;

namespace Sample.ConsoleApp.Context;

public class ContextData : IContextBpmnProcess, IExclusiveGateWay, IMessageReceiveTask
{
    public string IdBpmnProcess { get; init; } = Common.Constants.IdBpmnProcessingMain;

    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();

    public int TestValue { get; set; } = 0;

    public string TestValue2 { get; set; } = string.Empty;

    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();

    public ConcurrentDictionary<string, Type> RegistrationMessagesType { get; init; } = new();

    public ConcurrentDictionary<Type, object> ReceivedMessage { get; init; } = new();

}