using BpmnDotNet.Handlers;
using BpmnDotNet.Interfaces.Handlers;

namespace BpmnDotNetTests.Handlers;

public class CheckBpmnProcessDtoTests
{
    private readonly IXmlBpmnLoader _xmlBpmnLoader;
    private readonly ICheckBpmnProcessDto _checkBpmnProcessDto;


    public CheckBpmnProcessDtoTests()
    {
        _xmlBpmnLoader = new XmlBpmnLoader();
        _checkBpmnProcessDto = new CheckBpmnProcessDto();
    }

    [Fact]
    public void Check_CheckAvailabilityOutgoingsStartEvent_throwException()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/StartEvent_1.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Process_0aej3d1 Outgoing elements must have exactly one outgoing element. StartEvent_1", exception.Message);
    }

    [Fact]
    public void Check_CheckAvailabilityOutgoingsSendTask_throwException()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/SendTask_2.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Process_1mzblmf Outgoing elements must have exactly one outgoing element. ServiceTask_demo", exception.Message);
    }
    
    [Fact]
    public void Check_CheckAvailabilityIncomingPathReceiveTask_throwException()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/ReceiveTask_3.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Process_1pyjawa Outgoing elements must have exactly one incoming element. Activity_0c20yl4", exception.Message);
    }
    
    [Fact]
    public void Check_CheckAvailabilityIncomingPathSubProcess_throwException()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/SubProcess_4.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Process_1dnlu33 Outgoing elements must have exactly one incoming element. Activity_074iqn0", exception.Message);
    }
    
    [Fact]
    public void Check_CheckNormalBpmn_successfullyCompleted()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/diagram_1.bpmn");

        _checkBpmnProcessDto.Check(diagram);
    }
    
    [Fact]
    public void Check_CheckBeginingEvent_throwException()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/CheckBeginningAndEnd_5.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Process_0tl7ir6 Invalid count of elements start event", exception.Message);
    }
    
    [Fact]
    public void Check_CheckExclusiveGateway_throwException()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/ExclusiveGateway_6.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Process_0if2f5l Invalid count of elements ExclusiveGateway", exception.Message);
    }
    
    [Fact]
    public void Check_CheckParallelGateway_throwException()
    {
        var diagram = _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/ParallelGateway_7.bpmn");

        var exception = Assert.Throws<InvalidDataException>(() => _checkBpmnProcessDto.Check(diagram));

        Assert.Equal("Process_1kpdmnn Invalid count of elements ParallelGateway", exception.Message);
    }
    
    [Fact]
    public void Check_CheckFailIdProcess_throwException()
    {
        var exception = Assert.Throws<InvalidDataException>(() => _xmlBpmnLoader.LoadXml("./BpmnDiagram/CheckError/FailId_8.bpmn"));

        Assert.Equal("Not Find ID from:bpmn:process", exception.Message);
    }

    
}