namespace BpmnDotNet.Arm.Core.DiagramBuilder;

using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
/// Создаст элемент старт процесса.
/// </summary>
public class StartEventBuilder : IFullBodiedFigure<StartEventBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();
    private string _id = string.Empty;
    private int _xElement;
    private int _yElement;
    private string _titleText = string.Empty;
    private string _color = string.Empty; // TODO: почему-то не используется! посмотреть возможно ошибка.

    /// <inheritdoc />
    public string BuildSvg()
    {
        var hider = $"\t<g data-element-id=\"{_id}\" style=\"display: block;\" " +
                    $"transform=\"matrix(1 0 0 1 {_xElement} {_yElement})\">";

        var footer = "\t</g>";
        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine($"<title>{_titleText}</title>");
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public StartEventBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public StartEventBuilder AddPositionOffsets(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }

    /// <inheritdoc />
    public StartEventBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public StartEventBuilder AddTitle(string? titleText)
    {
        if (titleText is not null)
        {
            _titleText = titleText;
        }

        return this;
    }

    /// <inheritdoc />
    public StartEventBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }
}