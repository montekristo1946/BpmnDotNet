using System;

namespace BpmnDotNet.Common.Dto;

/// <summary>
///     Dto -отвечающая за историю отработки блоков.
/// </summary>
public class HistoryNodeState
{
    /// <summary>
    ///     ID, IdBpmnProcess+TokenProcess
    /// </summary>
    public string Id => $"{IdBpmnProcess}_{TokenProcess}_{DateCreated.Ticks}";

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
    public DateTime DateCreated { get; set; } = DateTime.Now;

    /// <summary>
    ///     Текущее состояние нод.
    /// </summary>
    public NodeJobStatus[] NodeStaus { get; set; } = [];
}