namespace BpmnDotNet.Abstractions.Elements;

/// <summary>
///     Путь откуда уходит управление.
/// </summary>
public interface IOutgoingPath
{
    string[] Outgoing { get; }
}