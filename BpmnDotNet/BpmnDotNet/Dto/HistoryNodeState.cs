namespace BpmnDotNet.Dto;

/// <summary>
///     Dto -отвечающая за историю отработки блоков.
/// </summary>
public class HistoryNodeState : HistoryNodeStateBase
{
    /// <summary>
    ///     Gets текущее состояние нод.
    /// </summary>
    public NodeJobStatus[] NodeStaus { get; init; } = [];

    /// <summary>
    ///     Gets сообщения об ошибках.
    /// </summary>
    public string[] ArrayMessageErrors { get; init; } = [];
}