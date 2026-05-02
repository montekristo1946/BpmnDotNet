using AutoFixture;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.Handlers;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class ElementOperatorTest
{
    [Fact]
    public void GetOutgoingPath_WhenOutgoingExists_ShouldReturnOutgoingPath()
    {
        // Arrange
        var currentNode = Substitute.For<IOutgoingPath, IElement>();
        var expectedOutgoing = new string[] 
        { 
            "element1",
            "element2"
        };
        
        // Настройка свойств IElement
        ((IElement)currentNode).IdElement.Returns("flow-123");
        ((IElement)currentNode).ElementType.Returns(ElementType.SequenceFlow);
        
        // Настройка свойств IOutgoingPath
        currentNode.Outgoing.Returns(expectedOutgoing);

        // Act
        var result = ElementOperator.GetOutgoingPath((IElement)currentNode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOutgoing, result.Outgoing);
        Assert.Equal(2, result.Outgoing.Length);
    }
    
    
    [Fact]
    public void GetOutgoingPath_WhenOutgoingIsNull_ShouldThrowInvalidDataException()
    {
        // Arrange
        var currentNode = Substitute.For<IOutgoingPath, IElement>();
        var elementId = "task-123";
        var elementType = ElementType.ServiceTask;
        
        ((IElement)currentNode).IdElement.Returns(elementId);
        ((IElement)currentNode).ElementType.Returns(elementType);
        currentNode.Outgoing.Returns((string[])null!);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            ElementOperator.GetOutgoingPath((IElement)currentNode));
        
        Assert.Contains("ServiceTask",exception.Message);
        Assert.Contains(elementId, exception.Message);
        Assert.Contains("Outgoing", exception.Message);
    }
    
    [Fact]
    public void GetOutgoingPath_WhenOutgoingIsEmpty_ShouldThrowInvalidDataException()
    {
        // Arrange
        var currentNode = Substitute.For<IOutgoingPath, IElement>();
        var elementId = "gateway-456";
        var elementType = ElementType.ExclusiveGateway;
        
        ((IElement)currentNode).IdElement.Returns(elementId);
        ((IElement)currentNode).ElementType.Returns(elementType);
        currentNode.Outgoing.Returns(Array.Empty<string>());

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            ElementOperator.GetOutgoingPath((IElement)currentNode));
        
        Assert.Contains("ExclusiveGateway", exception.Message);
        Assert.Contains(elementId, exception.Message);
        Assert.Contains("Outgoing", exception.Message);
    }
    
    [Fact]
    public void GetOutgoingPath_ShouldCastCurrentNodeToIOutgoingPath()
    {
        // Arrange
        var currentNode = Substitute.For<IOutgoingPath, IElement>();
        currentNode.Outgoing.Returns([string.Empty]);
        
        ((IElement)currentNode).IdElement.Returns("flow-001");
        ((IElement)currentNode).ElementType.Returns(ElementType.SequenceFlow);

        // Act
        var result = ElementOperator.GetOutgoingPath((IElement)currentNode);

        // Assert
        Assert.IsAssignableFrom<IOutgoingPath>(result);
        Assert.Same(currentNode, result); // Проверяем, что возвращается тот же объект
    }
    
    [Fact]
    public void GetOutgoingPath_WithAutoFixture_ShouldWork()
    {
        // Arrange
        var fixture = new Fixture();
        var currentNode = Substitute.For<IOutgoingPath, IElement>();
        var outgoingElements = fixture.CreateMany<string>(3).ToArray();
        
        var elementId = fixture.Create<string>();
        var elementType = fixture.Create<ElementType>();
        
        ((IElement)currentNode).IdElement.Returns(elementId);
        ((IElement)currentNode).ElementType.Returns(elementType);
        currentNode.Outgoing.Returns(outgoingElements);

        // Act
        var result = ElementOperator.GetOutgoingPath((IElement)currentNode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Outgoing.Length);
        Assert.Equal(outgoingElements, result.Outgoing);
    }
    
    
    [Theory]
    [InlineData(ElementType.StartEvent, "StartEvent-1")]
    [InlineData(ElementType.EndEvent, "EndEvent-1")]
    [InlineData(ElementType.SequenceFlow, "SequenceFlow-1")]
    [InlineData(ElementType.ExclusiveGateway, "ExclusiveGateway-1")]
    [InlineData(ElementType.ParallelGateway, "ParallelGateway-1")]
    [InlineData(ElementType.ServiceTask, "ServiceTask-1")]
    [InlineData(ElementType.SendTask, "SendTask-1")]
    [InlineData(ElementType.ReceiveTask, "ReceiveTask-1")]
    [InlineData(ElementType.SubProcess, "SubProcess-1")]
    public void GetOutgoingPath_WhenInvalid_ShouldIncludeElementInfoInMessage(
        ElementType elementType, string elementId)
    {
        // Arrange
        var currentNode = Substitute.For<IOutgoingPath, IElement>();
        
        ((IElement)currentNode).IdElement.Returns(elementId);
        ((IElement)currentNode).ElementType.Returns(elementType);
        currentNode.Outgoing.Returns(Array.Empty<string>());

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            ElementOperator.GetOutgoingPath((IElement)currentNode));
        
        Assert.Contains(elementType.ToString(), exception.Message);
        Assert.Contains(elementId, exception.Message);
    }
    
    [Theory]
    [InlineData(ElementType.StartEvent, "StartEvent-1")]
    [InlineData(ElementType.EndEvent, "EndEvent-1")]
    [InlineData(ElementType.SequenceFlow, "SequenceFlow-1")]
    [InlineData(ElementType.ExclusiveGateway, "ExclusiveGateway-1")]
    [InlineData(ElementType.ParallelGateway, "ParallelGateway-1")]
    [InlineData(ElementType.ServiceTask, "ServiceTask-1")]
    [InlineData(ElementType.SendTask, "SendTask-1")]
    [InlineData(ElementType.ReceiveTask, "ReceiveTask-1")]
    [InlineData(ElementType.SubProcess, "SubProcess-1")]
    public void GetIncomingPath_WhenInvalid_ShouldIncludeElementInfoInMessage(
        ElementType elementType, string elementId)
    {
        // Arrange
        var currentNode = Substitute.For<IIncomingPath, IElement>();
        
        ((IElement)currentNode).IdElement.Returns(elementId);
        ((IElement)currentNode).ElementType.Returns(elementType);
        currentNode.Incoming.Returns([]);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            ElementOperator.GetIncomingPath((IElement)currentNode));
        
        Assert.Contains(elementType.ToString(), exception.Message);
        Assert.Contains(elementId, exception.Message);
        Assert.Contains("IncomingPath", exception.Message);
    }
    
    [Fact]
    public void GetIncomingPath_WhenIncomingExists_ShouldReturnIncomingPath()
    {
        // Arrange
        var currentNode = Substitute.For<IIncomingPath, IElement>();
        var expectedIncoming = new string[] 
        { 
            "element1",
            "element2"
        };
        
        // Настройка свойств IElement
        ((IElement)currentNode).IdElement.Returns("task-123");
        ((IElement)currentNode).ElementType.Returns(ElementType.ServiceTask);
        
        // Настройка свойств IIncomingPath
        currentNode.Incoming.Returns(expectedIncoming);

        // Act
        var result = ElementOperator.GetIncomingPath((IElement)currentNode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedIncoming, result.Incoming);
        Assert.Equal(2, result.Incoming.Length);
    }
    
    [Fact]
    public void GetIncomingPath_WhenIncomingIsNull_ShouldThrowInvalidDataException()
    {
        // Arrange
        var currentNode = Substitute.For<IIncomingPath, IElement>();
        var elementId = "gateway-456";
        var elementType = ElementType.ExclusiveGateway;
        
        ((IElement)currentNode).IdElement.Returns(elementId);
        ((IElement)currentNode).ElementType.Returns(elementType);
        currentNode.Incoming.Returns((string[])null!);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            ElementOperator.GetIncomingPath((IElement)currentNode));
        
        Assert.Contains(elementType.ToString(), exception.Message);
        Assert.Contains(elementId, exception.Message);
        Assert.Contains("IncomingPath", exception.Message);
    }
    
}
