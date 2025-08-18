using System;
using System.Collections.Generic;

namespace BpmnDotNet.Common.Dto;

/// <summary>
/// Dto -отвечающая за историю отработки блоков.
/// </summary>
public class HistoryNodeState
{
    /// <summary>
    /// Id, IdBpmnProcess+TokenProcess
    /// </summary>
    public string Id => $"{IdBpmnProcess}_{TokenProcess}";

    /// <summary>
    /// ID Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; set; } = string.Empty;

    /// <summary>
    /// Текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; set; } = string.Empty;

    /// <summary>
    /// Время создания документа.
    /// </summary>
    public DateTime DateCreated => DateTime.Now;


    /// <summary>
    /// Очередь для вызовов.
    /// </summary>
    // private readonly Dictionary<string, NodeJobStatus> _nodeStateRegistry = new();
}