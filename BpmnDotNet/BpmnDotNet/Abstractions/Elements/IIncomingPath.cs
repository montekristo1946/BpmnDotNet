namespace BpmnDotNet.Abstractions.Elements;

/// <summary>
///     Путь куда приходит управление.
/// </summary>
public interface IIncomingPath
{
    /// <summary>
    /// Gets перечисления входных flow.
    /// </summary>
    string[] Incoming { get; }
}