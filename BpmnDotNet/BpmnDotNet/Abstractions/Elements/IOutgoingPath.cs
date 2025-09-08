namespace BpmnDotNet.Abstractions.Elements;

/// <summary>
///     Путь откуда уходит управление.
/// </summary>
public interface IOutgoingPath
{
    /// <summary>
    /// Gets перечисление выходных flow.
    /// </summary>
    string[] Outgoing { get; }
}