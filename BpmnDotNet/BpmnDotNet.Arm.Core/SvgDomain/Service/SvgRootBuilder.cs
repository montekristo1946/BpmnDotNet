namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Собрать блок root svg.
/// </summary>
public class SvgRootBuilder : IBpmnBuild<SvgRootBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();
    private string _id = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var hider = $"<svg id=\"{_id}\" width=\"100%\" height=\"100%\" xmlns=\"http://www.w3.org/2000/svg\">";
        var footer = "</svg>";

        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public SvgRootBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public SvgRootBuilder AddId(string id)
    {
        _id = id;
        return this;
    }
}