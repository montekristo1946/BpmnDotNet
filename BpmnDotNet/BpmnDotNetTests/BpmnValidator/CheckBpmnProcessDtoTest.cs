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
        _checkBpmnProcessDto = new CheckBpmnProcessDto();
    }

    [Fact]
    internal void HasStartAndEndEvents_CheckNotFindEndEvents_Exception()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/NotEnd_9.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto
            .HasStartAndEndEvents(diagram.ElementsFromBody, diagram.IdBpmnProcess));

        Assert.Equal(
            "Not EndEvent element found BpmnDotNet.BPMNDiagram.BpmnNatation.EndEventComponent: Process_1frdact",
            exception.Message);
    }

    [Fact]
    internal void HasStartAndEndEvents_CheckNotFindStartEvents_Exception()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/NotStart_10.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto
            .HasStartAndEndEvents(diagram.ElementsFromBody, diagram.IdBpmnProcess));

        Assert.Equal(
            "Not StartEvent element found BpmnDotNet.BPMNDiagram.BpmnNatation.StartEventComponent: Process_0mjkbbx",
            exception.Message);
    }

    [Fact]
    internal void HasOneTarget_FullPass_Exception()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/StartEvent_1.bpmn");

        var idProcess = "Process_0mjkbbx";
        var exception = Assert.Throws<InvalidDataException>(() =>
            _checkBpmnProcessDto.HasOneTarget(diagram.ElementsFromBody, idProcess));

        Assert.Equal(
            "[CheckBpmnProcessDto:HasOneTarget] Process_0mjkbbx Outgoing elements must have exactly one target element. StartEvent_1",
            exception.Message);
    }
}