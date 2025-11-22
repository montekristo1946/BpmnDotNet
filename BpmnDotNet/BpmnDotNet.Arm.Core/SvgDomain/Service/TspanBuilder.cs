using System.Runtime.CompilerServices;
using System.Text;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

[assembly: InternalsVisibleTo("BpmnDotNet.Arm.Core.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Arm.Core.SvgDomain.Service;

/// <summary>
/// Создаст многострочный текст.
/// </summary>
public class TspanBuilder : IBpmnBuild<TspanBuilder>
{
    private const int FontSize = 11;
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private int _symbolInOneLine = 15;
    private int _paddingY = 0;
    private int _paddingX = 0;
    private string _id = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var allLines = _childElements.SelectMany(SplitLinesFromWhiteSpace).ToArray();
        allLines = SplitLinesFromLongLine(allLines);

        for (var i = 0; i < allLines.Length; i++)
        {
            var body = allLines[i];
            var y = (i * FontSize) + FontSize + _paddingY;
            var x = _paddingX;
            var hider = $"<tspan id=\"{_id}\" x=\"{x}\" y=\"{y}\">";
            var footer = " </tspan>";
            _svgStorage.Append(hider);
            _svgStorage.Append(body);
            _svgStorage.Append(footer);
        }

        return _svgStorage.ToString();
    }

    /// <inheritdoc />
    public TspanBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    /// <inheritdoc />
    public TspanBuilder AddId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Максимальная длинна строки.
    /// </summary>
    /// <param name="len">Длинна.</param>
    /// <returns>Обьект создания.</returns>
    public TspanBuilder AddMaxLenLine(int len)
    {
        _symbolInOneLine = len;
        return this;
    }

    /// <summary>
    /// Смещение по оси Y.
    /// </summary>
    /// <param name="value">value.</param>
    /// <returns>Обьект создания.</returns>
    public TspanBuilder AddPaddingY(int value)
    {
        _paddingY = value;
        return this;
    }

    /// <summary>
    /// Смещение по оси X.
    /// </summary>
    /// <param name="value">value.</param>
    /// <returns>Обьект создания.</returns>
    public TspanBuilder AddPaddingX(int value)
    {
        _paddingX = value;
        return this;
    }

    /// <summary>
    /// Разделит строки по заданной длине.
    /// </summary>
    /// <param name="allLines">Строки на разденеия.</param>
    /// <returns>Вернет строки.</returns>
    internal string[] SplitLinesFromLongLine(string[] allLines)
    {
        var retArr = new List<string>();
        foreach (var line in allLines)
        {
            if (line.Length <= _symbolInOneLine)
            {
                retArr.Add(line);
                continue;
            }

            for (int i = 0; i < line.Length; i += _symbolInOneLine)
            {
                var length = Math.Min(_symbolInOneLine, line.Length - i);
                retArr.Add(line.Substring(i, length));
            }
        }

        return retArr.ToArray();
    }

    /// <summary>
    /// Разделит строку по пробелам.
    /// </summary>
    /// <param name="input">Входная строка.</param>
    /// <returns>Новые строки.</returns>
    internal string[] SplitLinesFromWhiteSpace(string input)
    {
        var arrWord = input.Split(' ');
        var segments = arrWord.Aggregate(
            new List<StringBuilder> { new() },
            (list, str) =>
                {
                    var last = list.Last();
                    if (last.Length + str.Length <= _symbolInOneLine)
                    {
                        last.Append($"{str} ");
                    }
                    else
                    {
                        list.Add(new StringBuilder($"{str} "));
                    }

                    return list;
                })
            .Select(sb => sb.ToString())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();

        return segments;
    }
}