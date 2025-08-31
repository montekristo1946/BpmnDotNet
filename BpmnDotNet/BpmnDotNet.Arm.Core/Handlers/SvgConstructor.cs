using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.DiagramBuilder;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Models;

namespace BpmnDotNet.Arm.Core.Handlers;

public class SvgConstructor : ISvgConstructor
{
    public Task<string> CreatePlane(BpmnPlane plane, SizeWindows sizeWindows)
    {
        var widthWindows = (int) sizeWindows.Width;
        var heightWindows =(int) sizeWindows.Height;
        if (plane.Shapes.Any() is false)
        {
            return Task.FromResult(string.Empty);
        }
        var shapes = CreateShapes(plane.Shapes,widthWindows,heightWindows);
        
        return Task.FromResult(shapes);
    }

    public Task<string> CreatePlane(BpmnPlane plane, NodeJobStatus[] nodeJobStatus, SizeWindows sizeWindows)
    {
        var widthWindows = (int) sizeWindows.Width;
        var heightWindows =(int) sizeWindows.Height;
        if (plane.Shapes.Any() is false)
        {
            return Task.FromResult(string.Empty);
        }
        
        var shapes = CreateColorShapes(plane.Shapes,nodeJobStatus,widthWindows,heightWindows);
        return Task.FromResult(shapes);
    }

    private string CreateColorShapes(BpmnShape[] shapes, NodeJobStatus[] nodeJobStatus, int widthWindows, int heightWindows)
    {
        var svgRootBuilder = IBpmnBuild<SvgRootBuilder>.Create();

        var scalingX = CalculateScalingViewportCoordinateX(shapes,widthWindows);
        var scalingY = CalculateScalingViewportCoordinateY(shapes,heightWindows);
       

        var viewportBuilder = IBpmnBuild<ViewportBuilder>
            .Create()
            .AddScalingX(scalingX)
            .AddScalingY(scalingY);
        
        const int stokeWidthStart = 2;
        const int stokeWidthEnd = 4;
        
        foreach (var shape in shapes)
        {
            var color  = GetColor(shape.BpmnElement,nodeJobStatus);
            var stringShape = shape.Type switch
            {
                ElementType.StartEvent => CreateStartEvent(shape, color, stokeWidthStart),
                ElementType.EndEvent => CreateStartEvent(shape, color, stokeWidthEnd),
                ElementType.SequenceFlow => CreateSequenceFlow(shape, color),
                ElementType.ServiceTask => CreateServiceTask(shape, color),
                ElementType.SendTask => CreateSendTask(shape, color),
                ElementType.ReceiveTask => CreateReceiveTask(shape, color),
                ElementType.ExclusiveGateway => CreateExclusiveGateway(shape, color),
                ElementType.ParallelGateway => CreateParallelGateway(shape, color),
                ElementType.SubProcess => CreateSubProcess(shape, color),
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

    private string GetColor(string shapeId, NodeJobStatus[] nodeJobStatus)
    {
        var state = nodeJobStatus.FirstOrDefault(s => s.IdNode == shapeId)?.ProcessingStaus ?? ProcessingStaus.None;
        var defaultColor =  "#22242a";
        var running = "#19aee8";
        var completed ="#00ae5e";
        var error = "#f34848";


        return state switch
        {
            ProcessingStaus.None => defaultColor,
            ProcessingStaus.Pending => running,
            ProcessingStaus.Works => running,
            ProcessingStaus.Complete => completed,
            ProcessingStaus.Failed => error,
            ProcessingStaus.WaitingCompletedWays => running,
            ProcessingStaus.WaitingReceivedMessage => running,
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    private string CreateShapes(BpmnShape[] shapes, int widthWindows, int heightWindows)
    {
        var svgRootBuilder = IBpmnBuild<SvgRootBuilder>.Create();

        var scalingX = CalculateScalingViewportCoordinateX(shapes,widthWindows);
        var scalingY = CalculateScalingViewportCoordinateY(shapes,heightWindows);
       

        var viewportBuilder = IBpmnBuild<ViewportBuilder>
            .Create()
            .AddScalingX(scalingX)
            .AddScalingY(scalingY);
        
        const int stokeWidthStart = 2;
        const int stokeWidthEnd = 4;
        var color = "#22242a";
        foreach (var shape in shapes)
        {
            var stringShape = shape.Type switch
            {
                ElementType.StartEvent => CreateStartEvent(shape, color, stokeWidthStart),
                ElementType.EndEvent => CreateStartEvent(shape, color, stokeWidthEnd),
                ElementType.SequenceFlow => CreateSequenceFlow(shape, color),
                ElementType.ServiceTask => CreateServiceTask(shape, color),
                ElementType.SendTask => CreateSendTask(shape, color),
                ElementType.ReceiveTask => CreateReceiveTask(shape, color),
                ElementType.ExclusiveGateway => CreateExclusiveGateway(shape, color),
                ElementType.ParallelGateway => CreateParallelGateway(shape, color),
                ElementType.SubProcess => CreateSubProcess(shape, color),
                _ => string.Empty
                // _ => throw new ArgumentOutOfRangeException()
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
    

    private string CreateSubProcess(BpmnShape shape, string color)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(15)
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
            .Build();
        return task;
    }

    private string CreateParallelGateway(BpmnShape shape, string color)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var gateway = IBpmnBuild<ParallelGatewayBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .Build();
        return gateway;
    }

    private string CreateExclusiveGateway(BpmnShape shape, string color)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var gateway = IBpmnBuild<ExlusiveGatewayBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .Build();
        return gateway;
    }

    private string CreateReceiveTask(BpmnShape shape, string color)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(15)
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
            .Build();
        return task;
    }

    private string CreateSendTask(BpmnShape shape, string color)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(15)
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
            .Build();
        return task;
    }

    private string CreateServiceTask(BpmnShape shape, string color)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                               ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");

        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(14)
            .AddPaddingY(15)
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

    private string CreateSequenceFlow(BpmnShape shape, string color)
    {
        if (shape.Bounds.Length < 2)
            throw new ArgumentException("Shape must have at least 2 bounds");

        var id = shape.Id;
        var bounds = shape.Bounds;

        var circle = IBpmnBuild<SequenceFlowBuilder>
            .Create()
            .AddColor(color)
            .AddBound(bounds)
            .AddId(id)
            .Build();
        return circle;
    }


    private string CreateStartEvent(BpmnShape shape, string color, int stokeWidth)
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


        var startEvent = IBpmnBuild<StartEventBuilder>.Create();
        var retStringSvg = startEvent
            .AddId(id)
            .AddPosition(xStart, yStart)
            .AddChild(circle)
            .Build();


        return retStringSvg;
    }

    /*  private string CreateService(BpmnShape shape, string color)
      {
          throw new NotImplementedException();
      }

      private string CreateSequenceFlow(BpmnShape shape, string color)
      {
          if (shape.Bounds.Length < 2)
              return string.Empty;

          var idMarker = Guid.NewGuid().ToString();
          var arrPoints = shape.Bounds.Select(p => $"{p.X},{p.Y}L").ToArray();
          var pathPoints = string.Join("", arrPoints);


          var retStringSvg = "<g class=\"djs-group\">\n " +
                             "<g class=\"djs-element djs-connection\" data-element-id=\"Flow_to_BlockFirstHandler\" " +
                             "style=\"display: block;\">\n " +
                             "<g class=\"djs-visual\">\n " +
                             "<defs>\n " +
                             $"<marker id=\"{idMarker}\" viewBox=\"0 0 20 20\" refX=\"11\" refY=\"10\"\n" +
                             "markerWidth=\"10\" markerHeight=\"10\" orient=\"auto\">\n" +
                             "<path d=\"M 1 5 L 11 10 L 1 15 Z\"\n" +
                             $"style=\"stroke-linecap: round; stroke-linejoin: round; stroke: {color}; stroke-width: 1px; fill: {color};\">\n " +
                             "</path>\n " +
                             "</marker>\n " +
                             "</defs>\n <path data-corner-radius=\"5\"\n " +
                             $"style=\"fill: none; stroke-linecap: round; stroke-linejoin: round; stroke: {color}; stroke-width: 2px;" +
                             $"marker-end: url(&quot;#{idMarker}&quot;);\"\n  " +
                             $"d=\"M{pathPoints}\"></path>\n  </g>\n " +
                             " </g>\n</g>";
          return retStringSvg;
      }



      private string CreateRootSvgHeader(string child)
      {
          //TODO: тут добавить масштабирование в matrix.
          var retStringSvg =
              "<svg width=\"100%\" height=\"100%\" xmlns=\"http://www.w3.org/2000/svg\">\n\n  " +
              "<g class=\"viewport\" transform=\"matrix(1,0,0,1,0,0)\">\n     " +
              "<g class=\"lauer-root-1\">" +
              $" {child} " +
              " </g>\n</g>\n</svg>";
          return retStringSvg;
      }



      private string DrawLabel(BpmnShape shape, string color)
      {
          var x = shape.BpmnLabel.X;
          var y = shape.BpmnLabel.Y;

          var retLabel = "<g class=\"djs-group\">\n " +
                         "<g class=\"djs-element djs-shape\" data-element-id=\"StartEvent_1_label\" style=\"display: block;\"\n" +
                         $"transform=\"matrix(1 0 0 1 {x} {y})\">\n " +
                         "<g class=\"djs-visual\"><text lineHeight=\"1.2\" class=\"djs-label\"\n " +
                         $"style=\"font-family: Arial, sans-serif; font-size: 11px; font-weight: normal; fill: {color};\">\n " +
                         $"<tspan x=\"0\" y=\"10\">{shape.Name}</tspan>\n      " +
                         "</text></g>\n                <rect class=\"djs-hit djs-hit-all\" x=\"0\" y=\"0\" width=\"88\" height=\"27\"\n      " +
                         "style=\"fill: none; stroke-opacity: 0; stroke: white; stroke-width: 15px;\"></rect>\n " +
                         "<rect x=\"-5\" y=\"-5\" rx=\"4\" width=\"98\" height=\"37\" class=\"djs-outline\" style=\"fill: none;\"></rect>\n" +
                         "</g>\n </g>";

          return retLabel;
      }*/
}