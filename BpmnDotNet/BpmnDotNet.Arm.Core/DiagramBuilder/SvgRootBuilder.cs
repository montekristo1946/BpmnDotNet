using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class SvgRootBuilder : IBpmnBuild<SvgRootBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();

    public string Build()
    {
        var hider = "<svg width=\"1600px\" height=\"600px\" xmlns=\"http://www.w3.org/2000/svg\">";
        var footer = "</svg>";

        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    public SvgRootBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }
}