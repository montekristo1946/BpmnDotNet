namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Блок получения сообщений.
/// </summary>
public class ReceiveTaskBuilder : IFullBodiedFigure<ReceiveTaskBuilder>
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
        var mainRect = IBpmnBuild<RectBuilder>.Create().AddColor(_color).BuildSvg();

        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine($"<title>{_titleText}</title>");
        _svgStorage.AppendLine(mainRect);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(CreateEnvelope());

        var footer = "</g>";
        _svgStorage.AppendLine(footer);
        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public ReceiveTaskBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public ReceiveTaskBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    /// <inheritdoc />
    public ReceiveTaskBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public ReceiveTaskBuilder AddTitle(string? titleText)
    {
        if (titleText is not null)
        {
            _titleText = titleText;
        }

        return this;
    }

    /// <inheritdoc />
    public ReceiveTaskBuilder AddPositionOffsets(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }

    private string CreateEnvelope()
    {
        var retRes = "<path d=\"m 6.3,5.6000000000000005 l 0,12.6 l 18.900000000000002,0 l 0,-12.6 z l 9.450000000000001,5.4 l 9.450000000000001,-5.4\"" +
                     $"style=\"fill: white; stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; stroke-width: 1px;\"> </path>";
        return retRes;
    }
}