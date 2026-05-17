using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Handlers;

namespace BpmnDotNetTests.Handlers;

public class XmlSerializationBpmnDiagramSectionTests
{
    private readonly XmlSerializationBpmnDiagramSection _xmlSerializationProcessSection;

    public XmlSerializationBpmnDiagramSectionTests()
    {
        _xmlSerializationProcessSection = new XmlSerializationBpmnDiagramSection();
    }

    [Fact]
    public void LoadXmlBpmn_FillingAllFields_BpmnPlane()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlBpmnDiagram("./BpmnDiagram/diagram_1.bpmn");

        Assert.Equal("IdBpmnProcessingMain", diagram.Id);
        Assert.Equal(28, diagram.Shapes.Length);
        Assert.Equal("IdBpmnProcessingMain", diagram.IdBpmnProcess);
        Assert.Equal("Процесс тестовый", diagram.Name);
    }

    [Fact]
    public void LoadXmlBpmn_CheckFailGetNameProcess_Exception()
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            {
                _ = _xmlSerializationProcessSection
                    .LoadXmlBpmnDiagram("./BpmnDiagram/CheckError/LoadXmlBpmn_CheckFailGetNameProcess_Exception.bpmn");
            }
        );

        var message = "Not Find Get Name Process from:IdBpmnProcessingMain";
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void LoadXmlBpmnDiagram_CheckLoadTextAnnotation_BpmnPlane()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlBpmnDiagram("./BpmnDiagram/diagram_2.bpmn");
        
        Assert.Equal("Process_17.05.2026", diagram.Id);
        Assert.Equal(8, diagram.Shapes.Length);
    }
}