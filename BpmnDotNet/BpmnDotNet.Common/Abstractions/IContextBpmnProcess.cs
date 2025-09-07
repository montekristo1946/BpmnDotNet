namespace BpmnDotNet.Common.Abstractions;

/// <summary>
/// Интерфейс контекста процесса.
/// </summary>
public interface IContextBpmnProcess
{
    /// <summary>
    ///     Gets iD Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; }

    /// <summary>
    ///     Gets текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; }
}