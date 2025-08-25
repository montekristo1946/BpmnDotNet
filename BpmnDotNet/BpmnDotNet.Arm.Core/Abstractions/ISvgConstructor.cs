using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface ISvgConstructor
{
    Task<string> CreatePlane(BpmnPlane bpmnPlane);
}