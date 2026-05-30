using System.Drawing;
using BpmnDotNet.Arm.Core.Common;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.SvgDomain.Service;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.BPMNDiagram.Abstractions;
using BpmnDotNet.Handlers;
using AutoFixture;

namespace BpmnDotNet.Arm.Core.Tests.Handlers;

public class SvgConstructorTests
{
    private readonly BpmnPlane _string;
    private readonly SvgConstructor _svgConstructor;

    public SvgConstructorTests()
    {
        _svgConstructor = new SvgConstructor();
        var serialization = new XmlSerializationBpmnDiagramSection();
        _string = serialization.LoadXmlBpmnDiagram("./BpmnDiagram/diagram_1.bpmn");
    }

    [Fact]
    public async Task CreatePlanes_Svg()
    {
        var size = new SizeWindows()
        {
            Height = 603,
            Width = 1594
        };
        var result = await _svgConstructor.CreatePlaneAsync(_string, [],size, []);
        // await File.WriteAllTextAsync("/mnt/Disk_D/TMP/18.08.2025/svg/demo2.svg", res);
        var normalizedResult = result.TrimEnd();
        Assert.Equal(38774, normalizedResult.Length);
    }
    
    [Theory]
    [InlineData(100, 10, 50, 1)]
    [InlineData(100, 20, 120, 0.7142857142857143)]
    [InlineData(200, 50, 150, 1)]
    [InlineData(100, 40, 200, 0.4166666666666667)]
    public void CalculateScalingViewportCoordinateX_ShouldReturn_ExpectedScale(
        int windowWidth,
        int minX,
        int maxX,
        double expectedScale)
    {
        // Arrange
        var shapes = new IBpmnShape[]
        {
            new BpmnShape
            {
                Bounds = new Bound { X = minX }
            },
            new BpmnShape
            {
                Bounds = new Bound { X = maxX }
            }
        };

        // Act
        var result = _svgConstructor.CalculateScalingViewportCoordinateX(shapes, windowWidth);

        // Assert
        Assert.Equal(expectedScale, result, precision: 10);
    }
    
    [Theory]
    [InlineData(100, 10, 150, 100d / 160)]
    [InlineData(200, 10, 150, 1)]
    public void CalculateScalingViewportCoordinateX_UsesWaypointCoordinatesForBpmnEdge(
        int windowWidth,
        int minWaypointX,
        int maxWaypointX,
        double expected)
    {
        // Arrange
        var edge = new BpmnEdge
        {
            Waypoints =
            [
                new Waypoint { X = 50 },
                new Waypoint { X = maxWaypointX },
                new Waypoint { X = minWaypointX },
                new Waypoint { X = 100 }
            ]
        };

        var shapes = new IBpmnShape[]
        {
            edge
        };

        // Act
        var result = _svgConstructor.CalculateScalingViewportCoordinateX(shapes, windowWidth);

        // Assert
        Assert.Equal(expected, result, 10);
    }
    
    [Theory]
    [InlineData(100, 10, 50, 1)]
    [InlineData(100, 20, 120, 0.7142857142857143)]
    [InlineData(200, 50, 150, 1)]
    [InlineData(100, 40, 200, 0.4166666666666667)]
    public void CalculateScalingViewportCoordinateY_ShouldReturn_ExpectedScale(
        int heightWindows,
        int minY,
        int maxY,
        double expectedScale)
    {
        // Arrange
        var shapes = new IBpmnShape[]
        {
            new BpmnShape
            {
                Bounds = new Bound { Y = minY }
            },
            new BpmnShape
            {
                Bounds = new Bound { Y = maxY }
            }
        };

        // Act
        var result = _svgConstructor.CalculateScalingViewportCoordinateY(shapes, heightWindows);

        // Assert
        Assert.Equal(expectedScale, result, precision: 10);
    }
    
    [Theory]
    [InlineData(100, 10, 150, 100d / 160)]
    [InlineData(200, 10, 150, 1)]
    public void CalculateScalingViewportCoordinateY_UsesWaypointCoordinatesForBpmnEdge(
        int heightWindows,
        int minWaypointY,
        int maxWaypointY,
        double expected)
    {
        // Arrange
        var edge = new BpmnEdge
        {
            Waypoints =
            [
                new Waypoint { Y = 50 },
                new Waypoint { Y = minWaypointY },
                new Waypoint { Y = maxWaypointY },
                new Waypoint { Y = 100 }
            ]
        };

        var shapes = new IBpmnShape[]
        {
            edge
        };

        // Act
        var result = _svgConstructor.CalculateScalingViewportCoordinateY(shapes, heightWindows);

        // Assert
        Assert.Equal(expected, result, 10);
    }

    [Fact]
    public void CreateStartEvent_ShouldReturn_SvgWithCorrectAttributes()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "start_1",
            Bounds = new Bound { X = 10, Y = 20, Width = 40, Height = 40 }
        };

        var color = "#123456";
        var strokeWidth = 3;
        var title = "Start title";

        // Act
        var result = _svgConstructor.CreateStartEvent(shape, color, strokeWidth, title);

        // Assert
        Assert.Contains("data-element-id=\"start_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("transform=\"matrix(1 0 0 1 10 20)\"", result);

        var expectedRadius = shape.Bounds.Width / 2;
        Assert.Contains($"cx=\"{expectedRadius}\"", result);
        Assert.Contains($"cy=\"{expectedRadius}\"", result);
        Assert.Contains($"r=\"{expectedRadius}\"", result);
        Assert.Contains($"stroke: {color}", result);
        Assert.Contains($"stroke-width: {strokeWidth}px", result);
    }

    [Fact]
    public void CreateStartEvent_ShouldThrow_WhenBoundsIsNull()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "start_null",
            Bounds = null
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateStartEvent(shape, "#000000", 1, "title"));
    }

    [Fact]
    public void CreateSequenceFlow_ShouldReturn_SvgWithPathAndMarker()
    {
        // Arrange
        var edge = new BpmnEdge
        {
            Id = "edge_1",
            Waypoints =
            [
                new Waypoint { X = 10, Y = 20 },
                new Waypoint { X = 30, Y = 40 }
            ],
            Name = "flow name"
        };

        var color = "#abcdef";
        var title = "Edge title";

        // Act
        var result = _svgConstructor.CreateSequenceFlow(edge, color, title);

        // Assert
        Assert.Contains("data-element-id=\"edge_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("<defs>", result);
        Assert.Contains("marker", result);
        Assert.Contains($"stroke: {color}", result);
        // path d should contain the two points in order
        Assert.Contains("M10,20L30,40L", result.Replace("\r", string.Empty));
    }

    [Fact]
    public void CreateSequenceFlow_ShouldThrow_WhenWaypointsLessThanTwo()
    {
        // Arrange
        var edge = new BpmnEdge
        {
            Id = "edge_bad",
            Waypoints = [new Waypoint { X = 1, Y = 2 }]
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _svgConstructor.CreateSequenceFlow(edge, "#000", "t"));
    }

    [Fact]
    public void CreateServiceTask_ShouldReturn_SvgUsingFixtureCreatedDto()
    {
        // Arrange
        var fixture = new Fixture();
        var bounds = new Bound { X = 5, Y = 6, Width = 100, Height = 60 };

        var shape = fixture.Build<BpmnShape>()
            .With(s => s.Bounds, bounds)
            .With(s => s.Id, "svc_1")
            .With(s => s.Name, "ServiceName")
            .Create();

        var color = "#00ff00";
        var title = "Service Title";

        // Act
        var result = _svgConstructor.CreateServiceTask(shape, color, title);

        // Assert
        Assert.Contains("data-element-id=\"svc_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("<rect", result);
        Assert.Contains("stroke: ", result);
        Assert.Contains(shape.Name, result);
    }
    
    
    [Fact]
    public void CreateServiceTask_ShouldThrow_ArgumentOutOfRangeExceptionWithParamName()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "svc_null",
            Bounds = null
        };

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateServiceTask(shape, "#000000", "title"));

        // Assert
        var expectedParamName = $"{shape.Id}:{nameof(shape.Bounds)}, {shape.Id}";
        Assert.Equal(expectedParamName, ex.ParamName);
    }

    [Fact]
    public void CreateSendTask_ShouldReturn_SvgWithEnvelopePath()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "send_1",
            Name = "SendName",
            Bounds = new Bound { X = 12, Y = 34, Width = 80, Height = 50 }
        };

        var color = "#ff8800";
        var title = "Send Title";

        // Act
        var result = _svgConstructor.CreateSendTask(shape, color, title);

        // Assert
        Assert.Contains("data-element-id=\"send_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("<rect", result);
        Assert.Contains("<path d=\"m 5.984999999999999,4.997999999999999 l 0,14 l 21,0 l 0,-14 z l 10.5,6 l 10.5,-6\"", result);
        Assert.Contains("fill: #ff8800", result);
        Assert.Contains(shape.Name, result);
    }

    [Fact]
    public void CreateSendTask_ShouldThrow_WhenBoundsIsNull()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "send_null",
            Bounds = null
        };

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateSendTask(shape, "#000000", "title"));

        // Assert
        var expectedParamName = $"{shape.Id}:{nameof(shape.Bounds)}, {shape.Id}";
        Assert.Equal(expectedParamName, ex.ParamName);
    }

    [Fact]
    public void CreateExclusiveGateway_ShouldReturn_SvgWithDiamondAndInnerBody()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "exclusive_1",
            Bounds = new Bound { X = 7, Y = 9, Width = 50, Height = 50 }
        };

        var color = "#8822cc";
        var title = "Exclusive Title";

        // Act
        var result = _svgConstructor.CreateExclusiveGateway(shape, color, title);

        // Assert
        Assert.Contains("data-element-id=\"exclusive_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("<polygon points=\"25,0 50,25 25,50 0,25\"", result);
        Assert.Contains($"fill: white; fill-opacity: 0.95;", result);
        Assert.Contains($"stroke: {color}; stroke-width: 2px;", result);
        Assert.Contains($"fill: {color}; stroke-linecap: round; stroke-linejoin: round; stroke: #8822cc; stroke-width: 1px", result);
    }

    [Fact]
    public void CreateExclusiveGateway_ShouldThrow_WhenBoundsIsNull()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "exclusive_null",
            Bounds = null
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateExclusiveGateway(shape, "#000000", "title"));
    }

    [Fact]
    public void CreateParallelGateway_ShouldReturn_SvgWithDiamondAndParallelBody()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "parallel_1",
            Bounds = new Bound { X = 11, Y = 13, Width = 60, Height = 60 }
        };

        var color = "#338855";
        var title = "Parallel Title";

        // Act
        var result = _svgConstructor.CreateParallelGateway(shape, color, title);

        // Assert
        Assert.Contains("data-element-id=\"parallel_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("<polygon points=\"25,0 50,25 25,50 0,25\"", result);
        Assert.Contains($"style=\"stroke-linecap: round; stroke-linejoin: round; stroke: {color}; stroke-width: 2px; fill: white; fill-opacity: 0.95;\"", result);
        Assert.Contains("<path d=\"m 23,10 0,12.5 -12.5,0 0,5 12.5,0 0,12.5 5,0 0,-12.5 12.5,0 0,-5 -12.5,0 0,-12.5 -5,0 z\"", result);
        Assert.Contains($"fill: {color}; stroke-linecap: round; stroke-linejoin: round; stroke: {color}; stroke-width: 1px;\"", result);
    }

    [Fact]
    public void CreateParallelGateway_ShouldThrow_WhenBoundsIsNull()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "parallel_null",
            Bounds = null
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateParallelGateway(shape, "#000000", "title"));
    }

    [Fact]
    public void CreateSubProcess_ShouldReturn_SvgWithInnerSquareAndPath()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "subprocess_1",
            Name = "SubprocessName",
            Bounds = new Bound { X = 15, Y = 25, Width = 120, Height = 80 }
        };

        var color = "#4477cc";
        var title = "Subprocess Title";

        // Act
        var result = _svgConstructor.CreateSubProcess(shape, color, title);

        // Assert
        Assert.Contains("data-element-id=\"subprocess_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("<rect", result);
        Assert.Contains("<rect x=\"0\" y=\"0\" width=\"14\" height=\"14\"", result);
        Assert.Contains("data-marker=\"sub-process\"", result);
        Assert.Contains($"stroke: {color}; stroke-width: 2px;", result);
        Assert.Contains(shape.Name, result);
    }

    [Fact]
    public void CreateSubProcess_ShouldThrow_WhenBoundsIsNull()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "subprocess_null",
            Bounds = null
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateSubProcess(shape, "#000000", "title"));
    }

    [Fact]
    public void CreateReceiveTask_ShouldReturn_SvgWithEnvelopePath()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "receive_1",
            Name = "ReceiveName",
            Bounds = new Bound { X = 23, Y = 45, Width = 90, Height = 55 }
        };

        var color = "#00aaff";
        var title = "Receive Title";

        // Act
        var result = _svgConstructor.CreateReceiveTask(shape, color, title);

        // Assert
        Assert.Contains("data-element-id=\"receive_1\"", result);
        Assert.Contains($"<title>{title}</title>", result);
        Assert.Contains("<rect", result);
        Assert.Contains("<path d=\"m 6.3,5.6000000000000005 l 0,12.6 l 18.900000000000002,0 l 0,-12.6 z l 9.450000000000001,5.4 l 9.450000000000001,-5.4\"", result);
        Assert.Contains($"stroke: {color}", result);
        Assert.Contains(shape.Name, result);
    }

    [Fact]
    public void CreateReceiveTask_ShouldThrow_WhenBoundsIsNull()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "receive_null",
            Bounds = null
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateReceiveTask(shape, "#000000", "title"));
    }

    [Fact]
    public void CreateAssociation_ShouldReturn_SvgWithDashedPath()
    {
        // Arrange
        var edge = new BpmnEdge
        {
            Id = "assoc_1",
            Name = "AssociationName",
            Waypoints =
            [
                new Waypoint { X = 5, Y = 5 },
                new Waypoint { X = 20, Y = 20 }
            ]
        };

        var color = "#551199";

        // Act
        var result = _svgConstructor.CreateAssociation(edge, color, "unused title");

        // Assert
        Assert.Contains("data-element-id=\"assoc_1\"", result);
        Assert.Contains("<path", result);
        Assert.Contains($"stroke: {color}; stroke-width: 1px; stroke-dasharray: 1,5;", result);
        Assert.Contains("marker-end", result);
        Assert.Contains("M5,5L20,20L", result);
    }

    [Fact]
    public void CreateAssociation_ShouldThrow_WhenWaypointsLessThanTwo()
    {
        // Arrange
        var edge = new BpmnEdge
        {
            Id = "assoc_bad",
            Waypoints =
            [
                new Waypoint { X = 1, Y = 2 }
            ]
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _svgConstructor.CreateAssociation(edge, "#000000", "title"));
    }

    [Fact]
    public void CreateTextAnnotation_ShouldReturn_SvgWithPathAndText()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "text_1",
            BpmnText = "Annotation text",
            Bounds = new Bound { X = 8, Y = 12, Width = 120, Height = 50 }
        };

        var color = "#aa2233";

        // Act
        var result = _svgConstructor.CreateTextAnnotation(shape, color);

        // Assert
        Assert.Contains("data-element-id=\"text_1\"", result);
        Assert.Contains("transform=\"matrix(1 0 0 1 8 12)\"", result);
        Assert.Contains("marker-end=\"url(#", result);
        Assert.Contains($"stroke=\"{color}\"", result);
        Assert.Contains(shape.BpmnText, result);
    }

    [Fact]
    public void CreateTextAnnotation_ShouldThrow_WhenBoundsIsNull()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "text_null",
            BpmnText = "Annotation text",
            Bounds = null
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _svgConstructor.CreateTextAnnotation(shape, "#000000"));
    }

    [Theory]
    [InlineData(BpmnDotNet.Dto.StatusType.None, "#22242a")]
    [InlineData(BpmnDotNet.Dto.StatusType.Pending, "#19aee8")]
    [InlineData(BpmnDotNet.Dto.StatusType.Works, "#19aee8")]
    [InlineData(BpmnDotNet.Dto.StatusType.Completed, "#319940")]
    [InlineData(BpmnDotNet.Dto.StatusType.Failed, "#f34848")]
    [InlineData(BpmnDotNet.Dto.StatusType.WaitingCompletedWays, "#19aee8")]
    [InlineData(BpmnDotNet.Dto.StatusType.WaitingReceivedMessage, "#19aee8")]
    public void GetColor_ShouldReturn_ExpectedColorForStatus(BpmnDotNet.Dto.StatusType statusType, string expectedColor)
    {
        // Arrange
        var statuses = new[]
        {
            new Dto.NodeJobStatus
            {
                IdNode = "task_1",
                StatusType = statusType
            }
        };

        // Act
        var result = _svgConstructor.GetColor("task_1", statuses);

        // Assert
        Assert.Equal(expectedColor, result);
    }

    [Fact]
    public void AddLabel_ShouldReturn_LabelSvg_WhenCoordinatesAreValid()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "label_1",
            Name = "LabelName",
            BpmnLabel = new Bound { X = 10, Y = 20, Width = 100, Height = 20 }
        };

        // Act
        var result = _svgConstructor.AddLabel(shape, "#112233");

        // Assert
        Assert.Contains("data-element-id=\"Label_label_1\"", result);
        Assert.Contains("transform=\"matrix(1 0 0 1 10 20)\"", result);
        Assert.Contains("<text", result);
        Assert.Contains("LabelName", result);
    }

    [Fact]
    public void AddLabel_ShouldReturn_EmptyString_WhenLabelCoordinatesAreNegative()
    {
        // Arrange
        var shape = new BpmnShape
        {
            Id = "label_negative",
            Name = "LabelName",
            BpmnLabel = new Bound { X = -1, Y = -1, Width = 100, Height = 20 }
        };

        // Act
        var result = _svgConstructor.AddLabel(shape, "#112233");

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
