using BpmnDotNet.BpmnEngineDomain.Dto;

namespace BpmnDotNet.HistoryDomain.Dto;

/// <summary>
///     Состояние узлов на Bpmn схеме.
/// </summary>
public class NodeJobStatus
{
    /// <summary>
    ///     Gets id узла на bpmn схемы.
    /// </summary>
    public string IdNode { get; init; } = string.Empty;

    /// <summary>
    ///     Gets состояние ноды на Bpmn схеме.
    /// </summary>
    public StatusNode StatusType { get; init; } = StatusNode.None;
}