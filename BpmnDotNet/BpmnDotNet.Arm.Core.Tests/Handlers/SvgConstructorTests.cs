using System.Drawing;
using BpmnDotNet.Arm.Core.Common;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.SvgDomain.Service;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.BPMNDiagram.Abstractions;
using BpmnDotNet.Handlers;

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
        Assert.Equal(39156, normalizedResult.Length);
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
}