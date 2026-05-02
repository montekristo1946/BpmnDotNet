using BpmnDotNet.Arm.Core.Common;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.SvgDomain.Service;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.Handlers;

namespace BpmnDotNet.Arm.Core.Tests.Handlers;

public class SvgConstructorTests
{
    private readonly BpmnPlane _string;
    private readonly ISvgConstructor _svgConstructor;

    public SvgConstructorTests()
    {
        _svgConstructor = new SvgConstructor();
        var serialization = new XmlSerializationBpmnDiagramSection();
        _string = serialization.LoadXmlBpmnDiagram("./BpmnDiagram/diagram_1.bpmn");
    }

    [Fact]
    public async Task CreatePlanes_Svg()
    {
        var size = new SizeWindows()
        {
            Height = 603,
            Width = 1594
        };
        var result = await _svgConstructor.CreatePlaneAsync(_string, [],size, []);
        // await File.WriteAllTextAsync("/mnt/Disk_D/TMP/18.08.2025/svg/demo2.svg", res);
        var normalizedResult = result.TrimEnd();
        Assert.Equal(38513, normalizedResult.Length);
    }
}