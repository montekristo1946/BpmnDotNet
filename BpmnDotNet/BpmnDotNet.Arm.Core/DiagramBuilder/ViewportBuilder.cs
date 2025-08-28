using System.Globalization;
using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class ViewportBuilder : IBpmnBuild<ViewportBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();
    private int _offsetX = 0;
    private int _offsetY = 0;
    private double _scalingX = 1;
    private double _scalingY = 1;
    public string Build()
    {
        var scallingX = _scalingX.ToString(CultureInfo.InvariantCulture);
        var scallingY = _scalingY.ToString(CultureInfo.InvariantCulture);
        var hider = $"<g class=\"viewport\" transform=\"matrix({scallingX},0,0,{scallingY},{_offsetX},{_offsetY})\">";
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

    public ViewportBuilder AddOffset(int offsetX, int offsetY)
    {
        _offsetX = offsetX;
        _offsetY = offsetY;
        return this;
    }

    public ViewportBuilder AddScalingX(double scalingX)
    {
        _scalingX = scalingX;
        return this;
    }

    public ViewportBuilder AddScalingY(double scalingY)
    {
        _scalingY = scalingY;
        return this;
    }
}