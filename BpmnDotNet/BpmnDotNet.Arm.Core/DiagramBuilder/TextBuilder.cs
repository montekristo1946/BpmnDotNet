using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class TextBuilder: IBpmnBuild<TextBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private string _color = string.Empty;
    
    public string Build()
    {
        var hider =
            $"<text lineHeight=\"1.2\" class=\"djs-label\" style=\"font-family: Arial, sans-serif; font-size: 11px; font-weight: normal; fill: {_color};\">";
        var footer = "</text>";
        
        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
        
    }

    public TextBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }
    
    public TextBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

}