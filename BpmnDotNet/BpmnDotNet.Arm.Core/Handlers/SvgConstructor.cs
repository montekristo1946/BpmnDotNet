using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.DiagramBuilder;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;
using BpmnDotNet.Common.Models;

namespace BpmnDotNet.Arm.Core.Handlers;

/// <inheritdoc />
public class SvgConstructor : ISvgConstructor
{
    /// <inheritdoc />
    public Task<string> CreatePlane(
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

        var shapes = nodeJobStatus.Any()
            ? CreateColorShapes(plane.Shapes, nodeJobStatus, widthWindows, heightWindows, descriptions)
            : CreateShapes(plane.Shapes, widthWindows, heightWindows, descriptions);

        return Task.FromResult(shapes);
    }

    private string CreateColorShapes(BpmnShape[] shapes, NodeJobStatus[] nodeJobStatus, int widthWindows,
        int heightWindows, DescriptionData[] descriptions)
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
            var color = GetColor(shape.BpmnElement, nodeJobStatus);
            var stringShape = shape.Type switch
            {
                ElementType.StartEvent => CreateStartEvent(shape, color, stokeWidthStart, title),
                ElementType.EndEvent => CreateStartEvent(shape, color, stokeWidthEnd, title),
                ElementType.SequenceFlow => CreateSequenceFlow(shape, color, title),
                ElementType.ServiceTask => CreateServiceTask(shape, color, title),
                ElementType.SendTask => CreateSendTask(shape, color, title),
                ElementType.ReceiveTask => CreateReceiveTask(shape, color, title),
                ElementType.ExclusiveGateway => CreateExclusiveGateway(shape, color, title),
                ElementType.ParallelGateway => CreateParallelGateway(shape, color, title),
                ElementType.SubProcess => CreateSubProcess(shape, color, title),
                _ => string.Empty
            };

            viewportBuilder.AddChild(stringShape);
            var label = AddLabel(shape, color);
            viewportBuilder.AddChild(label);
        }

        var viewportString = viewportBuilder.Build();
        svgRootBuilder.AddChild(viewportString);
        var retStringSvg = svgRootBuilder.Build();
        return retStringSvg;
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
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    private string CreateShapes(BpmnShape[] shapes, int widthWindows, int heightWindows, DescriptionData[] descriptions)
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
        var color = "#22242a";
        foreach (var shape in shapes)
        {
            var title = GetTitle(shape.BpmnElement, descriptions);
            var stringShape = shape.Type switch
            {
                ElementType.StartEvent => CreateStartEvent(shape, color, stokeWidthStart, title),
                ElementType.EndEvent => CreateStartEvent(shape, color, stokeWidthEnd, title),
                ElementType.SequenceFlow => CreateSequenceFlow(shape, color, title),
                ElementType.ServiceTask => CreateServiceTask(shape, color, title),
                ElementType.SendTask => CreateSendTask(shape, color, title),
                ElementType.ReceiveTask => CreateReceiveTask(shape, color, title),
                ElementType.ExclusiveGateway => CreateExclusiveGateway(shape, color, title),
                ElementType.ParallelGateway => CreateParallelGateway(shape, color, title),
                ElementType.SubProcess => CreateSubProcess(shape, color, title),
                _ => string.Empty
            };

            viewportBuilder.AddChild(stringShape);
            var label = AddLabel(shape, color);
            viewportBuilder.AddChild(label);
        }

        var viewportString = viewportBuilder.Build();
        svgRootBuilder.AddChild(viewportString);
        var retStringSvg = svgRootBuilder.Build();
        return retStringSvg;
    }

    private double CalculateScalingViewportCoordinateY(BpmnShape[] shapes, int heightWindows)
    {
        var maxY = shapes.SelectMany(p => p.Bounds).MaxBy(p => p.Y)?.Y ?? 0;
        var minY = shapes.SelectMany(p => p.Bounds).MinBy(p => p.Y)?.Y ?? 0;
        maxY += minY;
        if (maxY < heightWindows)
        {
            return 1;
        }

        var retValue = (double)heightWindows / maxY;
        return retValue;
    }

    private double CalculateScalingViewportCoordinateX(BpmnShape[] shapes, int widthWindows)
    {
        var maxX = shapes.SelectMany(p => p.Bounds).MaxBy(p => p.X)?.X ?? 0;
        var minX = shapes.SelectMany(p => p.Bounds).MinBy(p => p.X)?.X ?? 0;
        maxX += minX;
        if (maxX < widthWindows)
        {
            return 1;
        }

        var retValue = (double)widthWindows / maxX;
        return retValue;
    }


    private string CreateSubProcess(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(25)
            .AddPaddingX(10)
            .Build();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .Build();


        var task = IBpmnBuild<SubProcessBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .Build();
        return task;
    }

    private string CreateParallelGateway(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var gateway = IBpmnBuild<ParallelGatewayBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .Build();
        return gateway;
    }

    private string CreateExclusiveGateway(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var gateway = IBpmnBuild<ExlusiveGatewayBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .Build();
        return gateway;
    }

    private string CreateReceiveTask(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(25)
            .AddPaddingX(10)
            .Build();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .Build();


        var task = IBpmnBuild<ReceiveTaskBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .Build();
        return task;
    }

    private string CreateSendTask(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(25)
            .AddPaddingX(10)
            .Build();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .Build();


        var task = IBpmnBuild<SendTaskBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .Build();
        return task;
    }

    private string CreateServiceTask(BpmnShape shape, string color, string titleText)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(25)
            .AddPaddingX(10)
            .Build();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .Build();


        var task = IBpmnBuild<ServiceTaskBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .AddTitle(titleText)
            .Build();
        return task;
    }

    private string AddLabel(BpmnShape shape, string color)
    {
        if (shape.BpmnLabel.X < 0 || shape.BpmnLabel.Y < 0)
            return string.Empty;

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .Build();

        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .AddColor(color)
            .Build();

        var label = IBpmnBuild<LabelBuilder>
            .Create()
            .AddPosition(shape.BpmnLabel.X, shape.BpmnLabel.Y)
            .AddChild(textBuilder)
            .AddId($"Label_{shape.Id}")
            .Build();

        return label;
    }

    private string CreateSequenceFlow(BpmnShape shape, string color, string title)
    {
        if (shape.Bounds.Length < 2)
        {
            throw new ArgumentException("Shape must have at least 2 bounds");
        }

        var id = shape.Id;
        var bounds = shape.Bounds;

        var sequenceFlow = IBpmnBuild<SequenceFlowBuilder>
            .Create()
            .AddColor(color)
            .AddBound(bounds)
            .AddId(id)
            .AddTitle(title)
            .Build();
        return sequenceFlow;
    }


    private string CreateStartEvent(BpmnShape shape, string color, int stokeWidth, string title)
    {
        var boundCircle = shape.Bounds.FirstOrDefault()
                          ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var radius = boundCircle.Width / 2;
        var xStart = boundCircle.X;
        var yStart = boundCircle.Y;
        var id = shape.Id;

        var circle = IBpmnBuild<CircleBuilder>
            .Create()
            .AddColor(color)
            .AddRadius(radius)
            .AddStokeWidth(stokeWidth)
            .Build();


        var startEvent = IBpmnBuild<StartEventBuilder>
            .Create()
            .AddId(id)
            .AddPosition(xStart, yStart)
            .AddChild(circle)
            .AddTitle(title)
            .Build();

        return startEvent;
    }
}