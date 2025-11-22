namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

using System.Globalization;
using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

/// <summary>
/// Создаст видимую область SVG.
/// </summary>
public class ViewportBuilder : IBpmnBuild<ViewportBuilder>, IOffsetsPosition<ViewportBuilder>
{
    private readonly List<string> _childElements = new();
    private readonly StringBuilder _svgStorage = new();
    private int _offsetX = 0;
    private int _offsetY = 0;
    private double _scalingX = 1;
    private double _scalingY = 1;
    private string _id = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var scallingX = _scalingX.ToString(CultureInfo.InvariantCulture);
        var scallingY = _scalingY.ToString(CultureInfo.InvariantCulture);
        var hider =
            $"<g id=\"{_id}\" class=\"viewport\" transform=\"matrix({scallingX},0,0,{scallingY},{_offsetX},{_offsetY})\">";
        var footer = "</g>";

        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public ViewportBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public ViewportBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Добавить масштаб по оси Х.
    /// </summary>
    /// <param name="value">value.</param>
    /// <returns>Обьект сборки.</returns>
    public ViewportBuilder AddScalingX(double value)
    {
        _scalingX = value;
        return this;
    }

    /// <summary>
    /// Добавить масштаб по оси Y.
    /// </summary>
    /// <param name="value">value.</param>
    /// <returns>Обьект сборки.</returns>
    public ViewportBuilder AddScalingY(double value)
    {
        _scalingY = value;
        return this;
    }

    /// <inheritdoc />
    public ViewportBuilder AddPositionOffsets(int offsetX, int offsetY)
    {
        _offsetX = offsetX;
        _offsetY = offsetY;
        return this;
    }
}