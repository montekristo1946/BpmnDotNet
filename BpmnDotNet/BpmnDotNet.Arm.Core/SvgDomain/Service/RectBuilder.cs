namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Строитель квадратов.
/// </summary>
public class RectBuilder : IBpmnBuild<RectBuilder>, IColorBuilder<RectBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private string _color = string.Empty;
    private string _id = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var hider = $"<rect id=\"{_id}\" x=\"0\" y=\"0\" width=\"100\" height=\"80\" rx=\"10\" ry=\"10\" style=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 2px; fill: white; fill-opacity: 0.95;\">";
        var footer = "</rect>";

        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);
        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public RectBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public RectBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public RectBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }
}