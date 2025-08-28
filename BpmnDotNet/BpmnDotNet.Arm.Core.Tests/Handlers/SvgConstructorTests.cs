using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Arm.Core.Handlers;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Handlers;

namespace BpmnDotNet.Arm.Core.Tests.Handlers;

public class SvgConstructorTests
{
    private readonly BpmnPlane _bpmnPlane;
    private readonly ISvgConstructor _svgConstructor;

    public SvgConstructorTests()
    {
        _svgConstructor = new SvgConstructor();
        var serialization = new XmlSerializationBpmnDiagramSection();
        _bpmnPlane = serialization.LoadXmlBpmnDiagram("./BpmnDiagram/diagram_1.bpmn");
    }

    [Fact]
    public async Task CreatePlanes_Svg()
    {
        var size = new SizeWindows()
        {
            Height = 603,
            Width = 1594
        };
        var res = await _svgConstructor.CreatePlane(_bpmnPlane, size);
        await File.WriteAllTextAsync("/mnt/Disk_D/TMP/18.08.2025/svg/demo2.svg", res);

        throw new NotImplementedException();
    }
}