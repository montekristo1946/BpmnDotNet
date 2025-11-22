namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Создаст фигуру круг для SVG.
/// </summary>
public class CircleBuilder : IBpmnBuild<CircleBuilder>, IColorBuilder<CircleBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private string _color = string.Empty;
    private int _radius;
    private int _strokeWidth = 0;
    private string _id = string.Empty;

    /// <inheritdoc/>
    public string BuildSvg()
    {
        var hider = $"\t<circle id=\"{_id}\" cx=\"{_radius}\" cy=\"{_radius}\" r=\"{_radius}\"";

        var body = $"\t\tstyle=\"stroke-linecap: round; stroke-linejoin: round; stroke: {_color}; " +
                   $"stroke-width: {_strokeWidth}px; fill: white; fill-opacity: 0.95;\">";

        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        var footer = "\t</circle>";

        _svgStorage.AppendLine(hider);
        _svgStorage.AppendLine(body);
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public CircleBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public CircleBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Добавить размер круга.
    /// </summary>
    /// <param name="r">Величина радиуса.</param>
    /// <returns>Вернет билдер круга.</returns>
    public CircleBuilder AddRadius(int r)
    {
        _radius = r;
        return this;
    }

    /// <inheritdoc/>
    public CircleBuilder AddColor(string color)
    {
        _color = color;
        return this;
    }

    /// <summary>
    /// Задать толщину обьекта.
    /// </summary>
    /// <param name="strokeWidth">Тощина обьекта.</param>
    /// <returns>Обьект сборки.</returns>
    public CircleBuilder AddStokeWidth(int strokeWidth)
    {
        _strokeWidth = strokeWidth;
        return this;
    }
}