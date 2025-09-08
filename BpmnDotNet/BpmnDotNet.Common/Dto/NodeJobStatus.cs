namespace BpmnDotNet.Common.Dto;

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
    ///     Gets or sets состояние ноды на Bpmn схеме.
    /// </summary>
    public StatusType StatusType { get; set; } = StatusType.None;
}