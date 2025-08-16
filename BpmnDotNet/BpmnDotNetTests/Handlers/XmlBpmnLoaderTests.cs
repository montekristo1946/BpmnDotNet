using BpmnDotNet.Handlers;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;


namespace BpmnDotNetTests.Handlers;

public class XmlBpmnLoaderTests
{
    private readonly IXmlBpmnLoader _xmlBpmnLoader;

    public XmlBpmnLoaderTests()
    {
        _xmlBpmnLoader = new XmlBpmnLoader();
    }

    [Fact]
    public void LoadXmlBpmn()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/diagram_1.bpmn");

        Assert.NotNull(diagram);
        Assert.Equal(28, diagram.ElementsFromBody.Length);
    }
}