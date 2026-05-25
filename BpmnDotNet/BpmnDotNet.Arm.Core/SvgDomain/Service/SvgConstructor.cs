namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using BpmnDotNet.Arm.Core.Common;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.BPMNDiagram.Abstractions;
using BpmnDotNet.Dto;

/// <inheritdoc />
public class SvgConstructor : ISvgConstructor
{
    /// <inheritdoc />
    public Task<string> CreatePlaneAsync(
        BpmnPlane plane,
        NodeJobStatus[] nodeJobStatus,
        SizeWindows sizeWindows,
        DescriptionData[] descriptions)
    {
        var widthWindows = (int)sizeWindows.Width;
        var heightWindows = (int)sizeWindows.Height;
        if (plane.Shapes.Any() is false)
        {
            return Task.FromResult(string.Empty);
        }

        var shapes = CreateColorShapes(plane.Shapes, nodeJobStatus, widthWindows, heightWindows, descriptions);
        return Task.FromResult(shapes);
    }

    private string CreateColorShapes(
        IBpmnShape[] shapes,
        NodeJobStatus[] nodeJobStatus,
        int widthWindows,
        int heightWindows,
        DescriptionData[] descriptions)
    {
        var svgRootBuilder = IBpmnBuild<SvgRootBuilder>.Create();

        var scalingX = CalculateScalingViewportCoordinateX(shapes, widthWindows);
        var scalingY = CalculateScalingViewportCoordinateY(shapes, heightWindows);
        var minScale = Math.Min(scalingX, scalingY);

        var viewportBuilder = IBpmnBuild<ViewportBuilder>
            .Create()
            .AddScalingX(minScale)
            .AddScalingY(minScale);

        const int stokeWidthStart = 2;
        const int stokeWidthEnd = 4;

        foreach (var shape in shapes)
        {
            var title = GetTitle(shape.BpmnElement, descriptions);
            var color = "#22242a";
            if (nodeJobStatus.Any())
            {
                color = GetColor(shape.BpmnElement, nodeJobStatus);
            }

            var typeShape = shape switch
            {
                BpmnShape bpmnShape => bpmnShape.Type,
                BpmnEdge bpmnEdge => bpmnEdge.Type,
                _ => throw new InvalidOperationException(
                    $"Unsupported shape type: {shape.GetType().Name}. Expected BpmnShape or BpmnEdge."),
            };

            var stringShape = typeShape switch
            {
                ElementType.StartEvent => CreateStartEvent((BpmnShape)shape, color, stokeWidthStart, title),
                ElementType.EndEvent => CreateStartEvent((BpmnShape)shape, color, stokeWidthEnd, title),
                ElementType.SequenceFlow => CreateSequenceFlow((BpmnEdge)shape, color, title),
                ElementType.ServiceTask => CreateServiceTask((BpmnShape)shape, color, title),
                ElementType.SendTask => CreateSendTask((BpmnShape)shape, color, title),
                ElementType.ReceiveTask => CreateReceiveTask((BpmnShape)shape, color, title),
                ElementType.ExclusiveGateway => CreateExclusiveGateway((BpmnShape)shape, color, title),
                ElementType.ParallelGateway => CreateParallelGateway((BpmnShape)shape, color, title),
                ElementType.SubProcess => CreateSubProcess((BpmnShape)shape, color, title),
                ElementType.Association => CreateAssociation((BpmnEdge)shape, color, title),
                ElementType.TextAnnotation => CreateTextAnnotation((BpmnShape)shape, color),
                _ => string.Empty,
            };

            viewportBuilder.AddChild(stringShape);
            var label = AddLabel(shape, color);
            viewportBuilder.AddChild(label);
        }

        var viewportString = viewportBuilder.BuildSvg();
        svgRootBuilder.AddChild(viewportString);
        var retStringSvg = svgRootBuilder.BuildSvg();
        return retStringSvg;
    }

    private string CreateTextAnnotation(BpmnShape shape, string color)
    {
        var bound = shape.Bounds
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanAnnotationBuilder>
            .Create()
            .AddChild(shape.BpmnText)
            .AddWidthBlock(shape.Bounds.Width)
            .BuildSvg();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .BuildSvg();

        var task = IBpmnBuild<TextAnnotationBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddBounds(bound)
            .BuildSvg();
        return task;
    }

    private string CreateAssociation(BpmnEdge shape, string color, string title)
    {
        if (shape.Waypoints.Length < 2)
        {
            throw new ArgumentException("[SvgConstructor:CreateAssociation] Shape must have at least 2 bounds");
        }

        var id = shape.Id;
        var waypoints = shape.Waypoints;

        var association = IBpmnBuild<AssociationBuilder>
            .Create()
            .AddWayPoint(waypoints)
            .AddColor(color)
            .AddId(id)
            .BuildSvg();
        return association;
    }

    /// <summary>
    /// Рассчитать коэффициент масштабирования по оси Х.
    /// </summary>
    /// <param name="shapes">IBpmnShape.</param>
    /// <param name="widthWindows">Ширина текущего окна.</param>
    /// <returns>Масштаб.</returns>
    internal double CalculateScalingViewportCoordinateX(IBpmnShape[] shapes, int widthWindows)
    {
        var maxX = shapes.Select(shape =>
        {
            return shape switch
            {
                BpmnShape bpmnShape => bpmnShape.Bounds.X,
                BpmnEdge bpmnEdge => bpmnEdge.Waypoints.MaxBy(p => p.X)?.X ?? 0,
                _ => 0,
            };
        }).Max();

        var minX = shapes.Select(shape =>
        {
            return shape switch
            {
                BpmnShape bpmnShape => bpmnShape.Bounds.X,
                BpmnEdge bpmnEdge => bpmnEdge.Waypoints.MinBy(p => p.X)?.X ?? 0,
                _ => 0,
            };
        }).Min();

        var contentWidth = maxX + minX;

        return contentWidth < widthWindows
            ? 1
            : (double)widthWindows / contentWidth;
    }

    /// <summary>
    /// Рассчитать коэффициент масштабирования по оси Y.
    /// </summary>
    /// <param name="shapes">IBpmnShape.</param>
    /// <param name="heightWindows">Высота текущего окна.</param>
    /// <returns>Масштаб.</returns>
    internal double CalculateScalingViewportCoordinateY(IBpmnShape[] shapes, int heightWindows)
    {
        var maxY = shapes.Select(shape =>
        {
            return shape switch
            {
                BpmnShape bpmnShape => bpmnShape.Bounds.Y,
                BpmnEdge bpmnEdge => bpmnEdge.Waypoints.MaxBy(p => p.Y)?.Y ?? 0,
                _ => 0,
            };
        }).Max();

        var minY = shapes.Select(shape =>
        {
            return shape switch
            {
                BpmnShape bpmnShape => bpmnShape.Bounds.Y,
                BpmnEdge bpmnEdge => bpmnEdge.Waypoints.MinBy(p => p.Y)?.Y ?? 0,
                _ => 0,
            };
        }).Min();

        var contentHeight = maxY + minY;

        return contentHeight < heightWindows
            ? 1
            : (double)heightWindows / contentHeight;
    }

    private string GetTitle(string shapeBpmnElement, DescriptionData[] descriptions)
    {
        var title = descriptions.FirstOrDefault(p => p.TaskDefinitionId == shapeBpmnElement)?.Description ??
                    string.Empty;
        return title;
    }

    private string GetColor(string shapeId, NodeJobStatus[] nodeJobStatus)
    {
        var state = nodeJobStatus.FirstOrDefault(s => s.IdNode == shapeId)?.StatusType ?? StatusType.None;
        var defaultColor = "#22242a";
        var running = "#19aee8";
        var completed = "#319940";
        var error = "#f34848";

        return state switch
        {
            StatusType.None => defaultColor,
            StatusType.Pending => running,
            StatusType.Works => running,
            StatusType.Completed => completed,
            StatusType.Failed => error,
            StatusType.WaitingCompletedWays => running,
            StatusType.WaitingReceivedMessage => running,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private string CreateSubProcess(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddPaddingY(25)
            .AddBoundBlock(shape.Bounds)
            .BuildSvg();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .BuildSvg();

        var task = IBpmnBuild<SubProcessBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPositionOffsets(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .BuildSvg();
        return task;
    }

    private string CreateParallelGateway(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var gateway = IBpmnBuild<ParallelGatewayBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddPositionOffsets(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .BuildSvg();
        return gateway;
    }

    private string CreateExclusiveGateway(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var gateway = IBpmnBuild<ExlusiveGatewayBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddPositionOffsets(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .BuildSvg();
        return gateway;
    }

    private string CreateReceiveTask(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddPaddingY(20)
            .AddBoundBlock(shape.Bounds)
            .BuildSvg();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .BuildSvg();

        var task = IBpmnBuild<ReceiveTaskBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPositionOffsets(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .BuildSvg();
        return task;
    }

    private string CreateSendTask(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddPaddingY(20)
            .AddBoundBlock(shape.Bounds)
            .BuildSvg();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .BuildSvg();

        var task = IBpmnBuild<SendTaskBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPositionOffsets(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .BuildSvg();
        return task;
    }

    private string CreateServiceTask(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddPaddingY(20)
            .AddBoundBlock(shape.Bounds)
            .BuildSvg();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .BuildSvg();

        var task = IBpmnBuild<ServiceTaskBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPositionOffsets(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .BuildSvg();
        return task;
    }

    private string AddLabel(IBpmnShape shape, string color)
    {
        var labelShape = shape switch
        {
            BpmnShape bpmnShape => bpmnShape.BpmnLabel,
            BpmnEdge bpmnEdge => bpmnEdge.BpmnLabel,
            _ => throw new InvalidOperationException(
                $"Unsupported shape type: {shape.GetType().Name}. Expected BpmnShape or BpmnEdge."),
        };

        if (labelShape.X < 0 || labelShape.Y < 0)
        {
            return string.Empty;
        }

        var name = shape switch
        {
            BpmnShape bpmnShape => bpmnShape.Name,
            BpmnEdge bpmnEdge => bpmnEdge.Name,
            _ => throw new InvalidOperationException(
                $"Unsupported shape type: {shape.GetType().Name}. Expected BpmnShape or BpmnEdge."),
        };

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddBoundBlock(labelShape)
            .AddChild(name)
            .BuildSvg();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .BuildSvg();

        var label = IBpmnBuild<LabelBuilder>
            .Create()
            .AddPositionOffsets(labelShape.X, labelShape.Y)
            .AddChild(textBuilder)
            .AddId($"Label_{shape.Id}")
            .BuildSvg();

        return label;
    }

    private string CreateSequenceFlow(BpmnEdge shape, string color, string title)
    {
        if (shape.Waypoints.Length < 2)
        {
            throw new ArgumentException("Shape must have at least 2 bounds");
        }

        var id = shape.Id;
        var waypoints = shape.Waypoints;

        var sequenceFlow = IBpmnBuild<SequenceFlowBuilder>
            .Create()
            .AddColor(color)
            .AddWayPoint(waypoints)
            .AddId(id)
            .AddTitle(title)
            .BuildSvg();
        return sequenceFlow;
    }

    private string CreateStartEvent(BpmnShape shape, string color, int stokeWidth, string title)
    {
        var boundCircle = shape.Bounds
                          ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var radius = boundCircle.Width / 2;
        var xStart = boundCircle.X;
        var yStart = boundCircle.Y;
        var id = shape.Id;

        var startEvent = IBpmnBuild<StartEventBuilder>
            .Create()
            .AddId(id)
            .AddColor(color)
            .AddRadius(radius)
            .AddStokeWidth(stokeWidth)
            .AddPositionOffsets(xStart, yStart)
            .AddTitle(title)
            .BuildSvg();

        return startEvent;
    }
}