using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class CircleBuilder : IBpmnBuild<CircleBuilder>
{
    private string _color = string.Empty;
    private int _radius;
    private int _strokeWidth = 0;
    private readonly StringBuilder _svgStorage = new();
    
    public string Build()
    {
        var hider = $"\t<circle cx=\"{_radius}\" cy=\"{_radius}\" r=\"{_radius}\"";

        var body = $"\t\tstyle=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; " +
                   $"stroke-width: {_strokeWidth}px; fill: white; fill-opacity: 0.95;\">";
                  

        var footer = "\t</circle>";


        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine(body);
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    public CircleBuilder AddChild(string childElement)
    {
        return this;
    }
    
    public CircleBuilder AddRadius(int r)
    {
        _radius = r;
        return this;
    }

    public CircleBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    public CircleBuilder AddStokeWidth( int strokeWidth)
    {
        _strokeWidth = strokeWidth;
        return this;
    }
}