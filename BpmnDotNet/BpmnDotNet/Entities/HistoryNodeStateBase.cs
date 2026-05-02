namespace BpmnDotNet.Entities;

using BpmnDotNet.Dto;

/// <summary>
///     Dto -отвечающая за историю отработки блоков.
/// </summary>
public class HistoryNodeStateBase
{
    /// <summary>
    ///     Gets iD, IdBpmnProcess+TokenProcess.
    /// </summary>
    public string Id => $"{IdBpmnProcess}_{TokenProcess}";

    /// <summary>
    ///     Gets iD Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Gets текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Gets время создания документа.
    /// </summary>
    public long DateCreated { get; init; } = DateTime.Now.Ticks;

    /// <summary>
    ///     Gets время последнего изменения.
    /// </summary>
    public long DateLastModified { get; init; } = DateTime.MaxValue.Ticks;

    /// <summary>
    ///     Gets состояние процесса.
    /// </summary>
    public ProcessStatus ProcessStatus { get; init; } = ProcessStatus.None;
}