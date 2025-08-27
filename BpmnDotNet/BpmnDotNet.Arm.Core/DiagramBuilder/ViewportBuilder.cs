using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class ViewportBuilder : IBpmnBuild<ViewportBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();

    public string Build()
    {
        var hider = "<g class=\"viewport\" transform=\"matrix(1,0,0,1,0,0)\">";
        var footer = "</g>";

        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    public ViewportBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }
}