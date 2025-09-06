using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class ExlusiveGatewayBuilder : IBpmnBuild<ExlusiveGatewayBuilder>
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

        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine(CreatePolygon());
        _svgStorage.AppendLine(CreateBody());
        _childElements.ForEach(p => _svgStorage.AppendLine(p));

        var footer = "</g>";
        _svgStorage.AppendLine(footer);
        return _svgStorage.ToString();
    }

    private string CreateBody()
    {
        var retRes = "<path " +
                     "d=\"m 16,15 7.42857142857143,9.714285714285715 -7.42857142857143,9.714285714285715 3.428571428571429,0 5.714285714285715,-7.464228571428572 5.714285714285715,7.464228571428572 3.428571428571429,0 -7.42857142857143,-9.714285714285715 7.42857142857143,-9.714285714285715 -3.428571428571429,0 -5.714285714285715,7.464228571428572 -5.714285714285715,-7.464228571428572 -3.428571428571429,0 z\" " +
                     $"style=\"fill: {_color}; stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 1px;\"> " +
                     "</path>";
        return retRes;
    }

    private string CreatePolygon()
    {
        var retRes = "<polygon points=\"25,0 50,25 25,50 0,25\" " +
                     $"style=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 2px; fill: white; fill-opacity: 0.95;\"> " +
                     $"</polygon>";
        return retRes;
    }


    public ExlusiveGatewayBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    public ExlusiveGatewayBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    public ExlusiveGatewayBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    public ExlusiveGatewayBuilder AddPosition(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }
}