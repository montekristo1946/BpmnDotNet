namespace BpmnDotNet.Interfaces.Elements;

/// <summary>
/// Путь куда приходит управление.
/// </summary>
public interface IIncomingPath
{
    string[] Incoming { get; }
}