using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface ISvgConstructor
{
    Task<string> CreatePlane(BpmnPlane plane, SizeWindows sizeWindows);
    Task<string> CreatePlane(BpmnPlane plane, NodeJobStatus[] nodeJobStatus, SizeWindows sizeWindows);
}