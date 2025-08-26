using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class StartEventBuilder : IBpmnBuild<SvgRootBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();
    private string _color = string.Empty;
    private string _id = string.Empty;
    private int _radius;
    private int _xElement;
    private int _yElement;

    public string Build()
    {
        var space = "&nbsp;";
        
        var hider = "\t<g data-element-id=\"StartEvent_1\" style=\"display: block;\" " +
                    $"transform=\"matrix(1 0 0 1 {_xElement} {_yElement})\">";

        var circle = $"\t\t<circle cx=\"{_radius}\" cy=\"{_radius}\" r=\"{_radius}\"\n" +
                     $"\t\t\tstyle=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 2px; fill: white; fill-opacity: 0.95;\">\n" +
                     "\t\t</circle>";

        var footer = "\t</g>";

     
        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine(circle);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    public void AddChild(string childElement)
    {
        _childElements.Add(childElement);
    }

    public StartEventBuilder AddPosition(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }

    public StartEventBuilder AddRadius(int r)
    {
        _radius = r;
        return this;
    }

    public StartEventBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    public StartEventBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }
}