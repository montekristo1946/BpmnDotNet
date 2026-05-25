using System.Runtime.CompilerServices;
using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

[assembly: InternalsVisibleTo("BpmnDotNet.Arm.Core.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

/// <summary>
/// Создаст многострочный текст.
/// </summary>
public class TspanAnnotationBuilder : IBpmnBuild<TspanAnnotationBuilder>, ITspanAnnotation<TspanAnnotationBuilder>
{
    private const int FontSize = 12;
    private const int PixelOneSymbol = 7;
    private const int IndentSymbol = 1;

    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private int _widthBlock = 0;
    private string _id = string.Empty;
    private int _symbolInOneLine = 0;

    /// <inheritdoc />
    public string BuildSvg()
    {
        _symbolInOneLine = (_widthBlock / PixelOneSymbol) - IndentSymbol;

        var lines = _childElements.SelectMany(p => p.Split('\n')).ToArray();
        lines = SplitLongLine(lines, _symbolInOneLine);
        for (var i = 0; i < lines.Length; i++)
        {
            var body = lines[i];
            var y = (i * FontSize) + FontSize;
            var x = IndentSymbol * PixelOneSymbol;
            var hider = $"<tspan id=\"{_id}\" x=\"{x}\" y=\"{y}\">";
            var footer = "</tspan>";
            _svgStorage.Append(hider);
            _svgStorage.Append(body);
            _svgStorage.Append(footer);
        }

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public TspanAnnotationBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public TspanAnnotationBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <inheritdoc />
    public TspanAnnotationBuilder AddWidthBlock(int value)
    {
        _widthBlock = value;
        return this;
    }

    private string[] SplitLongLine(string[] lines, int maxSymbol)
    {
        var retArray = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.Length <= maxSymbol)
            {
                retArray.Add(line);
                continue;
            }

            var currentPosition = 0;

            while (currentPosition < line.Length)
            {
                var remainingLength = line.Length - currentPosition;

                if (remainingLength <= maxSymbol)
                {
                    retArray.Add(line.Substring(currentPosition));
                    break;
                }

                // Ищем последний пробел в пределах maxSymbol
                var endPosition = currentPosition + maxSymbol;
                var lastSpaceIndex = line.LastIndexOf(' ', endPosition);

                if (lastSpaceIndex <= currentPosition)
                {
                    // Нет подходящего пробела - режем по maxSymbol
                    retArray.Add(line.Substring(currentPosition, maxSymbol));
                    currentPosition += maxSymbol;
                }
                else
                {
                    // Режем по пробелу
                    retArray.Add(line.Substring(currentPosition, lastSpaceIndex - currentPosition));
                    currentPosition = lastSpaceIndex + 1;
                }
            }
        }

        return retArray.ToArray();
    }
}