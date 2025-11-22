namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Создать текст на SVG.
/// </summary>
public class TextBuilder : IBpmnBuild<TextBuilder>, IColorBuilder<TextBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private string _color = string.Empty;
    private string _id = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var hider =
            $"<text id=\"{_id}\" lineHeight=\"1.2\" class=\"djs-label\" style=\"font-family: Arial, sans-serif; font-size: 11px; font-weight: normal; fill: {_color};\">";
        var footer = "</text>";

        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public TextBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public TextBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public TextBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }
}