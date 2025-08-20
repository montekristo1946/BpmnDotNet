using BpmnDotNet.Common.Dto;
using BpmnDotNet.Handlers;

namespace BpmnDotNet.Dto;

public class BusinessProcessJobStatus
{
    /// <summary>
    /// Текущее состояние.
    /// </summary>
    public ProcessingStaus ProcessingStaus { get; set; } = ProcessingStaus.None;

    /// <summary>
    /// ID Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    /// Текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; } = string.Empty;

    // /// <summary>
    // /// Task основного потока.
    // /// </summary>
    public Task ProcessTask { get; init; } = Task.CompletedTask;

    /// <summary>
    /// Инстанс процесса.
    /// </summary>
    internal BusinessProcess Process { get; init; } = null!;
}