using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface ISvgConstructor
{
    Task<string> CreatePlane(BpmnPlane plane, SizeWindows sizeWindows);
}