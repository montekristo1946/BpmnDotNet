using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class SendTaskBuilder : IBpmnBuild<SendTaskBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private string _color = string.Empty;
    private string _id = string.Empty;
    private readonly List<string> _childElements = new();
    private int _xElement;
    private int _yElement;

    public string Build()
    {
        var hider =
            $"<g data-element-id=\"{_id}\" style=\"display: block;\" transform=\"matrix(1 0 0 1 {_xElement} {_yElement})\">";
        var mainRect = IBpmnBuild<RectBuilder>.Create().AddColor(_color).Build();

        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine(mainRect);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(CreateEnvelope());


        var footer = "</g>";
        _svgStorage.AppendLine(footer);
        return _svgStorage.ToString();
    }

    private string CreateEnvelope()
    {
        var retRes = "<path d=\"m 5.984999999999999,4.997999999999999 l 0,14 l 21,0 l 0,-14 z l 10.5,6 l 10.5,-6\" " +
                     $"style=\"fill: {_color}; stroke-linecap: round; stroke-linejoin: round; stroke: white; stroke-width: 1px;\"></path>";
        return retRes;
    }


    public SendTaskBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    public SendTaskBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    public SendTaskBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    public SendTaskBuilder AddPosition(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }
}