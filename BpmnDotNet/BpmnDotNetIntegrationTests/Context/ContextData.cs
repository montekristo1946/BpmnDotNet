using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;

namespace BpmnDotNetIntegrationTests.Context;

internal class ContextData : IContextBpmnProcess
{
    
    public string IdBpmnProcess { get; init; } = string.Empty;

    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();

    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();

    public ConcurrentDictionary<Type,string> RegistrationMessagesType { get; init; } = new();

    public ConcurrentDictionary<string, object> ReceivedMessage { get; init; } = new();
    
}