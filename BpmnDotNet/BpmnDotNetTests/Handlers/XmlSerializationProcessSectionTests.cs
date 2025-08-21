using BpmnDotNet.Handlers;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;


namespace BpmnDotNetTests.Handlers;

public class XmlSerializationProcessSectionTests
{
    private readonly IXmlSerializationProcessSection _xmlSerializationProcessSection;

    public XmlSerializationProcessSectionTests()
    {
        _xmlSerializationProcessSection = new XmlSerializationProcessSection();
    }

    [Fact]
    public void LoadXmlBpmn()
    {
        var diagram = _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/diagram_1.bpmn");

        Assert.NotNull(diagram);
        Assert.Equal(28, diagram.ElementsFromBody.Length);
        Assert.Equal("IdBpmnProcessingMain", diagram.IdBpmnProcess);
    }
}