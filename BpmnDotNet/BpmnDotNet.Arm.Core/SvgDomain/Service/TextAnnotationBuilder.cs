using System.Text;
using BpmnDotNet.BPMNDiagram;

namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Соберет TextAnnotation блок.
/// </summary>
internal class TextAnnotationBuilder :
    IColorBuilder<TextAnnotationBuilder>,
    IBpmnBuild<TextAnnotationBuilder>,
    IBoundsPosition<TextAnnotationBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private string _color = string.Empty;
    private string _id = string.Empty;
    private Bound _bound = new();

    /// <inheritdoc />
    public string BuildSvg()
    {
        var markerId = Guid.NewGuid();

        var start = (10, 0);
        var topLeft = (0, 0);
        var bottomLeft = (0, _bound.Height);
        var bottomRight = (10, _bound.Height);

        var pathData =
            $"M{start.Item1},{start.Item2} " +
            $"L{topLeft.Item1},{topLeft.Item2} " +
            $"L{bottomLeft.Item1},{bottomLeft.Height} " +
            $"L{bottomRight.Item1},{bottomRight.Height}";

        var svg = $$"""
                    <g
                        data-element-id="{{_id}}"
                        style="display: block;"
                        transform="matrix(1 0 0 1 {{_bound.X}} {{_bound.Y}})">
                        <path
                            d="{{pathData}}"
                            fill="none"
                            stroke="{{_color}}"
                            stroke-width="2"
                            stroke-linecap="round"
                            marker-end="url(#{{markerId}})" />
                    """;

        _svgStorage.AppendLine(svg);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine("</g>");

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public TextAnnotationBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    /// <inheritdoc />
    public TextAnnotationBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public TextAnnotationBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public TextAnnotationBuilder AddBounds(Bound bound)
    {
        _bound = bound;
        return this;
    }
}