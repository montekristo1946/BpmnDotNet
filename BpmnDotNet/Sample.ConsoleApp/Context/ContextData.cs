using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using Sample.ConsoleApp.Common;

namespace Sample.ConsoleApp.Context;

/// <summary>
/// Контекст тестового процесса. 
/// </summary>
internal class ContextData : IContextBpmnProcess
{
    /// <inheritdoc />
    public string IdBpmnProcess { get; init; } = Constants.IdBpmnProcessingMain;

    /// <inheritdoc />
    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();
    
    /// <inheritdoc />
    public ConcurrentDictionary<Type, string> RegistrationMessagesType { get; init; } = new();

    /// <inheritdoc />
    public ConcurrentDictionary<string, object>  ReceivedMessage { get; init; } = new();
    
    /// <summary>
    /// Тестовое поле.
    /// </summary>
    public int TestValue { get; set; } = 0;

    /// <summary>
    /// Тестовое поле.
    /// </summary>
    public string TestValue2 { get; set; } = string.Empty;
}