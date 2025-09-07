using BpmnDotNet.Common.Dto;

namespace BpmnDotNet.Dto;

internal class NodeTaskStatus
{
    public string IdNode { get; init; } = string.Empty;

    public StatusType StatusType { get; set; } = StatusType.None;
    
}