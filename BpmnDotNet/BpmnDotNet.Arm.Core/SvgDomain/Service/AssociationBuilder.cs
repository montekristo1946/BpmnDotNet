namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.BPMNDiagram;

/// <summary>
/// Строитель ассоциации на SVG.
/// </summary>
internal class AssociationBuilder :
    IColorBuilder<AssociationBuilder>,
    IBpmnBuild<AssociationBuilder>,
    IWayPointPosition<AssociationBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();
    private string _color = string.Empty;
    private string _id = string.Empty;
    private Waypoint[] _waypoints = [];

    /// <inheritdoc />
    public string BuildSvg()
    {
        var firstLine = $"<g data-element-id=\"{_id}\" style=\"display: block;\">";
        var marker = Guid.NewGuid();
        var arrPoints = _waypoints.Select(p => $"{p.X},{p.Y}L").ToArray();
        var pathPoints = string.Join(string.Empty, arrPoints);

        var path = $$"""
                     <path data-corner-radius="5" style="fill: none; stroke-linecap: round; stroke-linejoin: round; stroke: {{_color}}; stroke-width: 1px; stroke-dasharray: 1,5; stroke-linecap: round; marker-end: url(&quot;#{{marker}}&quot;);" 
                         d="M{{pathPoints}}"></path>
                     """;
        var footer = "</g>";

        _svgStorage.AppendLine(firstLine);
        _svgStorage.AppendLine(path);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public AssociationBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    /// <inheritdoc />
    public AssociationBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public AssociationBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public AssociationBuilder AddWayPoint(Waypoint[] waypoints)
    {
        _waypoints = waypoints;
        return this;
    }
}