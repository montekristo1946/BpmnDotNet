namespace BpmnDotNet.Arm.Core.DiagramBuilder;

using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
/// Построитель Labels.
/// </summary>
public class LabelBuilder : IBpmnBuild<LabelBuilder>, IOffsetsPosition<LabelBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();

    private int _xElement;
    private int _yElement;
    private string _id = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var hider = $"<g data-element-id=\"{_id}\" style=\"display: block;\" transform=\"matrix(1 0 0 1 {_xElement} {_yElement})\">";

        var footer = "</g>";

        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public LabelBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public LabelBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public LabelBuilder AddPositionOffsets(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }
}