namespace BpmnDotNet.Dto;

using BpmnDotNet.Common.Dto;
using BpmnDotNet.Handlers;

/// <summary>
/// Экземпляр описывающий запущенный процесс.
/// </summary>
public class BusinessProcessJobStatus
{
    /// <summary>
    ///     Gets or sets текущее состояние.
    /// </summary>
    public StatusType StatusType { get; set; } = StatusType.None;

    /// <summary>
    ///     Gets iD Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Gets текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; } = string.Empty;

    /// <summary>
    /// Gets task основного потока.
    /// </summary>
    public Task ProcessTask { get; init; } = Task.CompletedTask;

    /// <summary>
    ///     Gets инстанс процесса.
    /// </summary>
    internal BusinessProcess Process { get; init; } = null!;
}