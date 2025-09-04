using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class SubProcessBuilder : IBpmnBuild<SubProcessBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private string _color = string.Empty;
    private string _id = string.Empty;
    private readonly List<string> _childElements = new();
    private int _xElement;
    private int _yElement;

    public string Build()
    {
        var hider =
            $"<g data-element-id=\"{_id}\" style=\"display: block;\" transform=\"matrix(1 0 0 1 {_xElement} {_yElement})\">";
        var mainRect = IBpmnBuild<RectBuilder>.Create().AddColor(_color).Build();

        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine(mainRect);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(CreateInnerSquare());
        _svgStorage.AppendLine(CreateInnerPath());

        var footer = "</g>";
        _svgStorage.AppendLine(footer);
        return _svgStorage.ToString();
    }

    private string CreateInnerPath()
    {
        var retRes = "<path data-marker=\"sub-process\" d=\"m42.5,60 m 7,2 l 0,10 m -5,-5 l 10,0\" " +
                     $"style=\"fill: white; stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 2px;\"></path>";
        return retRes;
    }
    private string CreateInnerSquare()
    {
        var retRes = "<rect x=\"0\" y=\"0\" width=\"14\" height=\"14\" rx=\"0\" ry=\"0\"\n " +
                     $"style=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 1px; fill: white;\" transform=\"matrix(1 0 0 1 42.5 60)\"> </rect>";
        return retRes;
    }


    public SubProcessBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    public SubProcessBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    public SubProcessBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    public SubProcessBuilder AddPosition(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }
}