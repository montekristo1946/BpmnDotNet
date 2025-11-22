namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Параллельный гетвей.
/// </summary>
public class ParallelGatewayBuilder : IFullBodiedFigure<ParallelGatewayBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private string _color = string.Empty;
    private string _id = string.Empty;

    private int _xElement;
    private int _yElement;
    private string _titleText = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var hider =
            $"<g data-element-id=\"{_id}\" style=\"display: block;\" transform=\"matrix(1 0 0 1 {_xElement} {_yElement})\">";

        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine($"<title>{_titleText}</title>");
        _svgStorage.AppendLine(CreatePolygon());
        _svgStorage.AppendLine(CreateBody());
        _childElements.ForEach(p => _svgStorage.AppendLine(p));

        var footer = "</g>";
        _svgStorage.AppendLine(footer);
        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public ParallelGatewayBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public ParallelGatewayBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    /// <inheritdoc />
    public ParallelGatewayBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public ParallelGatewayBuilder AddPositionOffsets(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }

    /// <inheritdoc />
    public ParallelGatewayBuilder AddTitle(string? titleText)
    {
        if (titleText is not null)
        {
            _titleText = titleText;
        }

        return this;
    }

    private string CreateBody()
    {
        var retRes = "<path d=\"m 23,10 0,12.5 -12.5,0 0,5 12.5,0 0,12.5 5,0 0,-12.5 12.5,0 0,-5 -12.5,0 0,-12.5 -5,0 z\" " +
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
}