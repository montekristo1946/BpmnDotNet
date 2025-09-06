namespace BpmnDotNet.Common.Dto;

/// <summary>
///     Состояние узлов на Bpmn схеме.
/// </summary>
public class NodeJobStatus
{
    /// <summary>
    ///     Id узла на bpmn схемы.
    /// </summary>
    public string IdNode { get; init; } = string.Empty;

    /// <summary>
    ///     Состояние ноды на Bpmn схеме.
    /// </summary>
    public StatusType StatusType { get; set; } = StatusType.None;
}