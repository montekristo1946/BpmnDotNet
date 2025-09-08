namespace BpmnDotNet.Common.Dto;

using System;

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
    ///     Gets or sets iD Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets время создания документа.
    /// </summary>
    public long DateCreated { get; set; } = DateTime.Now.Ticks;

    /// <summary>
    ///     Gets or sets время последнего изменения.
    /// </summary>
    public long DateLastModified { get; set; } = DateTime.MaxValue.Ticks;

    /// <summary>
    ///     Gets or sets состояние процесса.
    /// </summary>
    public ProcessStatus ProcessStatus { get; set; } = ProcessStatus.None;
}