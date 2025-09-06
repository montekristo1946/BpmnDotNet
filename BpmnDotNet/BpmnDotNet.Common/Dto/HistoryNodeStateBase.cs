using System;

namespace BpmnDotNet.Common.Dto;

/// <summary>
///     Dto -отвечающая за историю отработки блоков.
/// </summary>
public class HistoryNodeStateBase
{
    /// <summary>
    ///     ID, IdBpmnProcess+TokenProcess
    /// </summary>
    public string Id => $"{IdBpmnProcess}_{TokenProcess}";

    /// <summary>
    ///     ID Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; set; } = string.Empty;

    /// <summary>
    ///     Текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; set; } = string.Empty;

    /// <summary>
    ///     Время создания документа.
    /// </summary>
    public long DateCreated { get; set; } = DateTime.Now.Ticks;

    /// <summary>
    ///     Время последнего изменения
    /// </summary>
    public long DateLastModified { get; set; } = DateTime.MaxValue.Ticks;

    /// <summary>
    ///     Состояние процесса.
    /// </summary>
    public ProcessStatus ProcessStatus { get; set; } = ProcessStatus.None;
}