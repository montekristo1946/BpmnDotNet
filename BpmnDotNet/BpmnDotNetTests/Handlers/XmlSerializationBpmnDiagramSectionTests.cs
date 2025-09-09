using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Handlers;

namespace BpmnDotNetTests.Handlers;

public class XmlSerializationBpmnDiagramSectionTests
{
    private readonly IXmlSerializationBpmnDiagramSection _xmlSerializationProcessSection;

    public XmlSerializationBpmnDiagramSectionTests()
    {
        _xmlSerializationProcessSection = new XmlSerializationBpmnDiagramSection();
    }

    [Fact]
    public void LoadXmlBpmn()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlBpmnDiagram("./BpmnDiagram/diagram_1.bpmn");

        Assert.Equal("IdBpmnProcessingMain", diagram.Id);
        Assert.Equal(28, diagram.Shapes.Length);
        Assert.Equal("IdBpmnProcessingMain", diagram.IdBpmnProcess);
    }
}