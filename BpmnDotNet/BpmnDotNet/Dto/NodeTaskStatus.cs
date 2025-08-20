using BpmnDotNet.Common.Dto;

namespace BpmnDotNet.Dto;

internal class NodeTaskStatus
{
    public string IdNode { get; init; } = String.Empty;

    public ProcessingStaus ProcessingStaus { get; set; } = ProcessingStaus.None;

    public Task? TaskRunNode { get; init; }
}