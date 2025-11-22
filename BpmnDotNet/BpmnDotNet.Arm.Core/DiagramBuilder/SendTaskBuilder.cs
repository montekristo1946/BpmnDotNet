namespace BpmnDotNet.Arm.Core.DiagramBuilder;

using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
/// Конструктор блока отправки сообщения.
/// </summary>
public class SendTaskBuilder : IFullBodiedFigure<SendTaskBuilder>
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
    public SendTaskBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public SendTaskBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    /// <inheritdoc />
    public SendTaskBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public SendTaskBuilder AddPositionOffsets(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }

    /// <inheritdoc />
    public SendTaskBuilder AddTitle(string? titleText)
    {
        if (titleText is not null)
        {
            _titleText = titleText;
        }

        return this;
    }

    private string CreateEnvelope()
    {
        var retRes = "<path d=\"m 5.984999999999999,4.997999999999999 l 0,14 l 21,0 l 0,-14 z l 10.5,6 l 10.5,-6\" " +
                     $"style=\"fill: {_color}; stroke-linecap: round; stroke-linejoin: round; stroke: white; stroke-width: 1px;\"></path>";
        return retRes;
    }
}