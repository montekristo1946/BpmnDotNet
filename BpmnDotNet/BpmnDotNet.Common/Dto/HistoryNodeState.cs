using System;

namespace BpmnDotNet.Common.Dto;

/// <summary>
///     Dto -отвечающая за историю отработки блоков.
/// </summary>
public class HistoryNodeState:HistoryNodeStateBase
{
    /// <summary>
    ///     Текущее состояние нод.
    /// </summary>
    public NodeJobStatus[] NodeStaus { get; set; } = [];
}