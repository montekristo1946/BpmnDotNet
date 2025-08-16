namespace BpmnDotNet.Interfaces.Elements;

/// <summary>
/// Путь откуда уходит управление.
/// </summary>
public interface IOutgoingPath
{
    string[] Outgoing { get; }
}