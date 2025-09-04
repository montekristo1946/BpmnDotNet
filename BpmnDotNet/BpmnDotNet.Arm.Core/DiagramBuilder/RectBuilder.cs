using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class RectBuilder : IBpmnBuild<RectBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private string _color = string.Empty;

    public string Build()
    {
        var hider = $"<rect x=\"0\" y=\"0\" width=\"100\" height=\"80\" rx=\"10\" ry=\"10\" style=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 2px; fill: white; fill-opacity: 0.95;\">";

        var footer = "</rect>";

        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine(footer);
        return _svgStorage.ToString();
    }

    public RectBuilder AddChild(string childElement)
    {
        return this;
    }

    public RectBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }
}