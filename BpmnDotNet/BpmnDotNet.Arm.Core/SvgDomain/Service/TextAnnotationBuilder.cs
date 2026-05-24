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

        var start = (_bound.X + 10, _bound.Y);
        var topLeft = (_bound.X, _bound.Y);
        var bottomLeft = (_bound.X, _bound.Y + _bound.Height);
        var bottomRight = (_bound.X + 10, _bound.Y + _bound.Height);

        var pathData =
            $"M{start.Item1},{start.Y} " +
            $"L{topLeft.X},{topLeft.Y} " +
            $"L{bottomLeft.X},{bottomLeft.Item2} " +
            $"L{bottomRight.Item1},{bottomRight.Item2}";

        var svg = $$"""
                    <g data-element-id="{{_id}}" style="display: block;">
                        <path
                            d="{{pathData}}"
                            fill="none"
                            stroke="{{_color}}"
                            stroke-width="2"
                            stroke-linecap="round"
                            marker-end="url(#{{markerId}})" />
                    """;

        _svgStorage.AppendLine(svg);
        _childElements.ForEach(p =>_svgStorage.AppendLine(p));
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