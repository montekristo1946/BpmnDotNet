using AutoFixture.Xunit2;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnValidator;
using BpmnDotNet.Dto;
using BpmnDotNet.Handlers;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;

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

    [Fact]
    internal void HasOneSource_CheckServiceTask_Exception()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/SendTask_2.bpmn");

        var idProcess = "Process_0mjkbbx";
        var exception = Assert.Throws<InvalidDataException>(() =>
            _checkBpmnProcessDto.HasOneTarget(diagram.ElementsFromBody, idProcess));

        Assert.Equal(
            "[CheckBpmnProcessDto:HasOneTarget] Process_0mjkbbx Outgoing elements must have exactly one target element. ServiceTask_demo",
            exception.Message);
    }

    [Fact]
    internal void Check_CheckManyInputFlowReceiveTask_NotException()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/ReceiveTask_3.bpmn");

        var exception = Record.Exception(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Null(exception);
    }

    [Fact]
    internal void Check_CheckManyInputFlowSubProcess_NotException()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/SubProcess_4.bpmn");

        var exception = Record.Exception(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Null(exception);
    }

    [Fact]
    internal void HasOneStartEven_CheckManyStart_Exception()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection(
                "./BpmnDiagram/CheckError/CheckBeginningAndEnd_5.bpmn");

        var idProcess = "Process_0mjkbbx";
        var exception = Assert.Throws<InvalidDataException>(() =>
            _checkBpmnProcessDto.HasOneStartEven(diagram.ElementsFromBody, idProcess));

        Assert.Equal("There should be only one StartEvent on the diagram, find: 2: Process_0mjkbbx", exception.Message);
    }
    
    
    [Fact]
    internal void Check_CheckOneWayParallelGateway_NotException()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/ParallelGateway_7.bpmn");

        var exception = Record.Exception(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Null(exception);
    }
    
    [Fact]
    internal void Check_CheckFailId_Exception()
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/FailId_8.bpmn"));

        Assert.Equal("Not Find ID from:bpmn:process", exception.Message);
    }
    
    [Fact]
    internal void Check_CheckNotEnd_NotException()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/NotEnd_9.bpmn");
        
        var exception = Assert.Throws<InvalidDataException>(() =>
            _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Not EndEvent element found BpmnDotNet.BPMNDiagram.BpmnNatation.EndEventComponent: Process_1frdact", exception.Message);
    }
    
    [Fact]
    internal void Check_CheckNotStartEven_NotException()
    {
        var diagram =
            _xmlSerializationProcessSection.LoadXmlProcessSection("./BpmnDiagram/CheckError/NotStart_10.bpmn");
        
        var exception = Assert.Throws<InvalidDataException>(() =>
            _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Not StartEvent element found BpmnDotNet.BPMNDiagram.BpmnNatation.StartEventComponent: Process_0mjkbbx", exception.Message);
    }
}