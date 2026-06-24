namespace BpmnDotNet.BpmnEngineDomain.Dto;

using BpmnDotNet.BpmnEngineDomain.Abstractions;

/// <summary>
/// Экземпляр описывающий запущенный процесс.
/// </summary>
public class BusinessProcessJobStatus
{
    /// <summary>
    /// Gets iD Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    /// Gets текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; } = string.Empty;

    /// <summary>
    /// Gets task основного потока.
    /// </summary>
    public Task ProcessTask { get; init; } = Task.CompletedTask;

    /// <summary>
    /// Gets инстанс процесса.
    /// </summary>
    internal IBpmnEngine Process { get; init; } = null!;
}