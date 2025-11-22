namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;

/// <summary>
/// Строитель стрелочек на SVG.
/// </summary>
public class SequenceFlowBuilder : IColorBuilder<SequenceFlowBuilder>, ITitleBuilder<SequenceFlowBuilder>, IBpmnBuild<SequenceFlowBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private Bound[] _bounds = [];
    private string _color = string.Empty;
    private string _id = string.Empty;
    private string _titleText = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var firstLine = $"<g data-element-id=\"{_id}\" style=\"display: block;\">";
        var marker = Guid.NewGuid();
        var arrPoints = _bounds.Select(p => $"{p.X},{p.Y}L").ToArray();
        var pathPoints = string.Join(string.Empty, arrPoints);

        var defs =
            $"\t<defs>\n" +
            $"\t\t<marker id=\"{marker}\" viewBox=\"0 0 20 20\" refX=\"11\" refY=\"10\" markerWidth=\"10\" markerHeight=\"10\" orient=\"auto\">\n" +
            $"\t\t<path d=\"M 1 5 L 11 10 L 1 15 Z\" style=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 1px; fill: {_color};\"></path>\n" +
            "\t\t</marker>\n" +
            "\t</defs>";

        var path =
            $"<path data-corner-radius=\"5\" style=\"fill: none; stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 2px; marker-end: url(&quot;#{marker}&quot;);\" " +
            $"d=\"M{pathPoints}\"></path>";
        var footer = "</g>";

        _svgStorage.AppendLine(firstLine);
        _svgStorage.AppendLine($"<title>{_titleText}</title>");
        _svgStorage.AppendLine(defs);
        _svgStorage.AppendLine(path);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public SequenceFlowBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public SequenceFlowBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <summary>
    /// Добавить гоацины.
    /// </summary>
    /// <param name="bounds">Bound.</param>
    /// <returns>Собираемая фигура.</returns>
    public SequenceFlowBuilder AddBound(Bound[] bounds)
    {
        _bounds = bounds;
        return this;
    }

    /// <inheritdoc />
    public SequenceFlowBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    /// <inheritdoc />
    public SequenceFlowBuilder AddTitle(string? titleText)
    {
        if (titleText is not null)
        {
            _titleText = titleText;
        }

        return this;
    }
}