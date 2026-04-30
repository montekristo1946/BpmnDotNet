using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Elements.BpmnNatation;
using BpmnDotNet.Handlers;
using BpmnDotNet.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class ContextDatTest : IContextBpmnProcess
{
    public string IdBpmnProcess { get; init; }
    public string TokenProcess { get; init; }
    public ConcurrentDictionary<string, string> ConditionRoute { get; init; } = new();
    public ConcurrentDictionary<string, Type> RegistrationMessagesType { get; init; } = new();
    public ConcurrentDictionary<Type, object> ReceivedMessage { get; init; } = new();
}

public class PathFinderTests
{
    private readonly PathFinder _pathFinder;
    private readonly ILogger<PathFinder> _logger;
    private readonly ContextDatTest _context;

    public PathFinderTests()
    {
        _logger = Substitute.For<ILogger<PathFinder>>();
        _pathFinder = new PathFinder(_logger);
        _context = new ContextDatTest();
    }

    [Fact]
    public void GetConditionRouteWithExclusiveGateWay_CheckFalsePath_InvalidDataException()
    {
        var idGateway = "idGateway";
        var incoming = new[] { "incomingLine1", "incomingLine2" };
        var outgoing = new[] { "outgoingLine1", "outgoingLine2" };
        _context.ConditionRoute.TryAdd(idGateway, "FailFlowValue");
        var element = new ExclusiveGatewayComponent(idGateway, incoming, outgoing);

        var exception = Assert.Throws<InvalidDataException>(() =>
            {
                var res = _pathFinder.GetConditionRouteWithExclusiveGateWay(_context, element);
            }
        );

        var message = " [GetConditionRouteWithExclusiveGateWay] There is no such way from gateway:idGateway";
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void GetConditionRouteWithExclusiveGateWay_CheckFalsePath_TruePath()
    {
        var idGateway = "idGateway";
        var incoming = new[] { "incomingLine1", "incomingLine2" };
        var outgoing = new[] { "outgoingLine1", "outgoingLine2" };
        var truePath = "outgoingLine2";
        _context.ConditionRoute.TryAdd(idGateway, truePath);
        var element = new ExclusiveGatewayComponent(idGateway, incoming, outgoing);

        var res = _pathFinder.GetConditionRouteWithExclusiveGateWay(_context, element);

        Assert.Equal(truePath, res);
    }
    
    [Fact]
    public void GetStartEvent_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        IElement[] elementsSrc = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _pathFinder.GetStartEvent(elementsSrc));
    }
    
    [Fact]
    public void GetStartEvent_WithEmptyArray_ThrowsInvalidDataException()
    {
        // Arrange
        var elementsSrc = Array.Empty<IElement>();

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => _pathFinder.GetStartEvent(elementsSrc));
        Assert.Equal(nameof(BpmnProcessDto), exception.Message);
    }
    
    [Fact]
    public void GetStartEvent_WithSingleStartEvent_ReturnsStartEvent()
    {
        // Arrange
        var elementsSrc = new IElement[]
        {
            new StartEventComponent("id",["firstName"]),
            new SequenceFlowComponent("id",["LastName"],["LastName"]),
        };

        // Act
        var result = _pathFinder.GetStartEvent(elementsSrc);

        // Assert
        Assert.Single(result);
        Assert.Equal("id",result[0].IdElement);
        Assert.Equal(ElementType.StartEvent, result[0].ElementType);
    }
    
    [Fact]
    public void GetStartEvent_CheckCountEvents_ThrowsInvalidDataException()
    {
        // Arrange
        var elementsSrc = new IElement[]
        {
            new EndEventComponent("id",["firstName"]),
            new SequenceFlowComponent("id",["LastName"],["LastName"]),
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => _pathFinder.GetStartEvent(elementsSrc));
        Assert.Equal("[GetStartEvent] not find StartEvent", exception.Message);
    }
    
    private IElement[] CreateElements(params ElementType[] types)
    {
        return types.Select((type, index) => CreateElement(type, $"element_{index}")).ToArray();
    }
    private IElement CreateElement(ElementType elementType, string? id = null, string[]? incoming = null, string[]? outgoing = null)
    {
        var elementId = id ?? Guid.NewGuid().ToString();
        var inc = incoming ?? [];
        var outg = outgoing ??  [];
    
        return elementType switch
        {
            ElementType.ReceiveTask => new ReceiveTaskComponent(elementId, inc, outg),
            ElementType.SendTask => new SendTaskComponent(elementId, inc, outg),
            ElementType.ServiceTask => new ServiceTaskComponent(elementId, inc, outg),
            ElementType.StartEvent => new StartEventComponent(elementId, outg),
            ElementType.EndEvent => new EndEventComponent(elementId, inc),
            ElementType.ExclusiveGateway => new ExclusiveGatewayComponent(elementId, inc, outg),
            ElementType.ParallelGateway => new ParallelGatewayComponent(elementId, inc, outg),
            ElementType.SubProcess => new SubProcessComponent(elementId, inc, outg),
            ElementType.SequenceFlow => new SequenceFlowComponent(elementId, inc, outg),
            _ => throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null)
        };
    }
    
    [Fact]
    public void GetNextNode_WhenElementsSrcIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IElement[] elementsSrc = null;
        var currentNodes = CreateElements(ElementType.StartEvent);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _pathFinder.GetNextNode(elementsSrc, currentNodes, _context));
    }
    
    [Fact]
    public void GetNextNode_WhenCurrentNodesIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var elementsSrc = CreateElements(ElementType.ServiceTask);
        IElement[] currentNodes = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _pathFinder.GetNextNode(elementsSrc, currentNodes, _context));
    }
    
    [Fact]
    public void GetNextNode_WhenContextIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var elementsSrc = CreateElements(ElementType.ServiceTask);
        var currentNodes = CreateElements(ElementType.StartEvent);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _pathFinder.GetNextNode(elementsSrc, currentNodes, null));
    }
    
    [Fact]
    public void GetNextNode_WhenElementsSrcIsEmpty_ThrowsInvalidDataException()
    {
        // Arrange
        var elementsSrc = Array.Empty<IElement>();
        var currentNodes = CreateElements(ElementType.StartEvent);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            _pathFinder.GetNextNode(elementsSrc, currentNodes, _context));
        Assert.Contains("elementsSrc", exception.Message);
    }
    
    [Fact]
    public void GetNextNode_WhenCurrentNodesIsEmpty_ThrowsInvalidDataException()
    {
        // Arrange
        var elementsSrc = CreateElements(ElementType.ServiceTask);
        var currentNodes = Array.Empty<IElement>();

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => 
            _pathFinder.GetNextNode(elementsSrc, currentNodes, _context));
        Assert.Contains("currentNodes", exception.Message);
    }
    
    [Theory]
    [InlineData(ElementType.StartEvent)]
    [InlineData(ElementType.ServiceTask)]
    [InlineData(ElementType.SendTask)]
    [InlineData(ElementType.ReceiveTask)]
    [InlineData(ElementType.SubProcess)]
    [InlineData(ElementType.ExclusiveGateway)]
    [InlineData(ElementType.ParallelGateway)]
    public void GetNextNode_ForPathBasedElementTypes_ReturnsNextNodesBasedOnPath(ElementType elementType)
    {
        // Arrange
        var idFlow = Guid.NewGuid().ToString();
        var idExpectedNextNode = Guid.NewGuid().ToString();
        var idCurrentNode = Guid.NewGuid().ToString();
        var flow = CreateElement(ElementType.SequenceFlow,idFlow,[ idCurrentNode],[idExpectedNextNode]);
        var currentNode = CreateElement(elementType, idCurrentNode,[ Guid.NewGuid().ToString()],[idFlow]);
        var expectedNextNode = CreateElement(ElementType.ServiceTask, idExpectedNextNode,[idFlow],[ Guid.NewGuid().ToString()]);
     
        var elementsSrc = new[] 
        { 
            currentNode, 
            expectedNextNode,
            CreateElement(ElementType.EndEvent, "other_1",[ Guid.NewGuid().ToString()],[ Guid.NewGuid().ToString()]),
            flow
        };
        
        var currentNodes = new[] { currentNode };

        // Act
        var result = _pathFinder.GetNextNode(elementsSrc, currentNodes, _context);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(expectedNextNode, result[0]);
    }
    
    [Fact]
    public void GetNextNode_ForPathEndEvent_Empty()
    {
        // Arrange
        var idFlow = Guid.NewGuid().ToString();
        var idCurrentNode = Guid.NewGuid().ToString();
        var currentNode = CreateElement(ElementType.EndEvent, idCurrentNode,[ Guid.NewGuid().ToString()],[idFlow]);
        var elementsSrc = new[] 
        { 
            currentNode, 
        };
        var currentNodes = new[] { currentNode };

        // Act
        var result = _pathFinder.GetNextNode(elementsSrc,currentNodes, _context);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
     
    }
}