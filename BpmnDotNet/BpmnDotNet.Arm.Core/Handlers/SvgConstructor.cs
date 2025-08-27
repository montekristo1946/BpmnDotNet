using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.DiagramBuilder;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Models;

namespace BpmnDotNet.Arm.Core.Handlers;

public class SvgConstructor : ISvgConstructor
{
    public Task<string> CreatePlane(BpmnPlane bpmnPlane)
    {
        var retStringSvg = new StringBuilder();

        var shapes = CreateShapes(bpmnPlane.Shapes);

        retStringSvg.Append(shapes);
        return Task.FromResult(retStringSvg.ToString());
    }

    private string CreateShapes(BpmnShape[] shapes)
    {
        var svgRootBuilder = IBpmnBuild<SvgRootBuilder>.Create();

        var viewportBuilder = IBpmnBuild<ViewportBuilder>.Create();
        ;

        const int stokeWidthStart = 2;
        const int stokeWidthEnd = 4;
        var color = "#22242a";
        foreach (var shape in shapes)
        {
            var stringShape = shape.Type switch
            {
                ElementType.StartEvent => CreateStartEvent(shape, color,stokeWidthStart),
                ElementType.EndEvent => CreateStartEvent(shape, color,stokeWidthEnd),
                ElementType.SequenceFlow => CreateSequenceFlow(shape, color),
                ElementType.ServiceTask => CreateServiceTask(shape, color),

                _ => string.Empty
                // _ => throw new ArgumentOutOfRangeException()
            };
            
            viewportBuilder.AddChild(stringShape);
            var label = AddLabel(shape);
            viewportBuilder.AddChild(label);
        }

        var viewportString = viewportBuilder.Build();
        svgRootBuilder.AddChild(viewportString);
        var retStringSvg = svgRootBuilder.Build();
        return retStringSvg;
    }

    private string CreateServiceTask(BpmnShape shape, string color)
    {
        var boundServiceTask = shape.Bounds.FirstOrDefault()
                          ?? throw new ArgumentOutOfRangeException($"{nameof(shape.Bounds)}, {shape.Id}");
        
        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .AddMaxLenLine(5)
            .AddPaddingY(15)
            .AddPaddingX(10)
            .Build();
        
        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .Build();
        
       
        var serviceTask = IBpmnBuild<ServiceTaskBuilder>
            .Create()
            .AddColor(color)
            .AddId(shape.Id)
            .AddChild(textBuilder)
            .AddPosition(boundServiceTask.X, boundServiceTask.Y)
            .Build();
        return serviceTask;
    }

    private string AddLabel(BpmnShape shape)
    {
        if(shape.BpmnLabel.X<0 || shape.BpmnLabel.Y<0)
            return string.Empty;
        
        var tspan = IBpmnBuild<TspanBuilder>
            .Create()
            .AddChild(shape.Name)
            .Build();
        
        var textBuilder = IBpmnBuild<TextBuilder>
            .Create()
            .AddChild(tspan)
            .Build();

        var label = IBpmnBuild<LabelBuilder>
            .Create()
            .AddPosition(shape.BpmnLabel.X,shape.BpmnLabel.Y)
            .AddChild(textBuilder)
            .Build();
        
        return label;
    }

    private string CreateSequenceFlow(BpmnShape shape, string color)
    {
        if (shape.Bounds.Length < 2)
            throw new ArgumentException("Shape must have at least 2 bounds");

        var id = shape.Id;
        var bounds =  shape.Bounds;
        
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