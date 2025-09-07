using System.Collections.Concurrent;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Elements.BpmnNatation;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class ContextDatTest : IContextBpmnProcess, IExclusiveGateWayRoute, IMessageReceiveTask
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
}