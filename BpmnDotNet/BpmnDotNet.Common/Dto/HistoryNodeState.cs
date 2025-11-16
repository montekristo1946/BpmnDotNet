namespace BpmnDotNet.Common.Dto;

using BpmnDotNet.Common.Entities;

/// <summary>
///     Dto -отвечающая за историю отработки блоков.
/// </summary>
public class HistoryNodeState : HistoryNodeStateBase
{
    /// <summary>
    ///     Gets or sets текущее состояние нод.
    /// </summary>
    public NodeJobStatus[] NodeStaus { get; set; } = [];

    /// <summary>
    ///     Gets or sets сообщения об ошибках.
    /// </summary>
    public string[] ArrayMessageErrors { get; set; } = [];
}