using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Common;

namespace BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Context;

public class ContextData : IContextBpmnProcess
{
    public string IdBpmnProcess { get; init; } = "BpmnClientTests";

    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();

    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();

    public ConcurrentDictionary<string, Type> RegistrationMessagesType { get; init; } = new();

    public ConcurrentDictionary<Type, object> ReceivedMessage { get; init; } = new();
    

    public string TestValue { get; set; } = string.Empty;
}