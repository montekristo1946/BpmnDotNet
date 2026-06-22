using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnValidator;
using BpmnDotNet.Handlers;

namespace BpmnDotNetTests.BpmnValidator;

public class CheckBpmnProcessDtoTest
{
    private readonly CheckBpmnProcessDto _checkBpmnProcessDto;
    private readonly IXmlSerializationProcessSection _xmlSerializationProcessSection;

    public CheckBpmnProcessDtoTest()
    {
        _xmlSerializationProcessSection = new XmlSerializationProcessSection();
        _checkBpmnProcessDto  = new CheckBpmnProcessDto();
    }

    [Fact]
    internal void HasStartAndEndEvents_CheckNotFindEndEvents_Exception()
    {
        
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/NotEnd_9.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Not EndEvent element found BpmnDotNet.BPMNDiagram.BpmnNatation.EndEventComponent: Process_1frdact",
            exception.Message);
    }
    
    [Fact]
    internal void HasStartAndEndEvents_CheckNotFindStartEvents_Exception()
    {
        
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/NotStart_10.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Not StartEvent element found BpmnDotNet.BPMNDiagram.BpmnNatation.StartEventComponent: Process_0mjkbbx",
            exception.Message);
    }
    
}