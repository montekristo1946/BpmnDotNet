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
    private const int OptimumLinesInActivityBloc = 5;
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private int _symbolInOneLine = 15;
    private int _paddingY = 0;
    private int _paddingX = 0;
    private string _id = string.Empty;

    /// <inheritdoc />
    public string BuildSvg()
    {
        var allLines = MergeAllChild(_childElements);
        allLines = RemoveLineBreakEntity(allLines);
        var splitWords = allLines.Split(' ');

        var isNeedRemap = CheckRemapLines(splitWords);
        if (isNeedRemap)
        {
            splitWords = RemapLines(splitWords);
            splitWords = SplitLinesFromLongLine(splitWords);
        }

        splitWords = ClearSpaceLine(splitWords);
        splitWords = RemoveTrailingSpaces(splitWords);

        for (var i = 0; i < splitWords.Length; i++)
        {
            var body = splitWords[i];
            var y = (i * FontSize) + FontSize + _paddingY;
            var x = _paddingX;
            var hider = $"<tspan id=\"{_id}\" x=\"{x}\" y=\"{y}\">";
            var footer = "</tspan>";
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
    /// <param name="allLines">Строки на разделение.</param>
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
    /// Переразметим строки.
    /// </summary>
    /// <param name="arrWord">Входная строка.</param>
    /// <returns>Новые строки.</returns>
    internal string[] RemapLines(string[] arrWord)
    {
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

    /// <summary>
    /// Уберет пустые строки.
    /// </summary>
    /// <param name="src">Входной массив.</param>
    /// <returns>Обработанный массив.</returns>
    internal string[] ClearSpaceLine(string[] src)
    {
        var reaArr = src.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return reaArr;
    }

    /// <summary>
    /// Объединит все строки в единую через пробел.
    /// </summary>
    /// <param name="elements">массив подстрок.</param>
    /// <returns>Единая строка.</returns>
    internal string MergeAllChild(List<string> elements)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < elements.Count; i++)
        {
            var element = elements[i];
            if (element is null)
            {
                continue;
            }

            sb.Append(element);
            if (i < elements.Count - 1)
            {
                sb.Append(" ");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Определит необходимость производить переразметку строк.
    /// </summary>
    /// <param name="splitWords">Массив строк.</param>
    /// <returns>Результат работы.</returns>
    internal bool CheckRemapLines(string[]? splitWords)
    {
        if (splitWords is null || splitWords.Length == 0)
        {
            return false;
        }

        if (splitWords.Length > OptimumLinesInActivityBloc)
        {
            return true;
        }

        var checkMaxLines = splitWords.Max(s => s.Length);

        return checkMaxLines > _symbolInOneLine;
    }

    /// <summary>
    /// Удалим пробелы в коцен строки.
    /// </summary>
    /// <param name="src">Исходные строки.</param>
    /// <returns>Обработанные строки.</returns>
    internal string[] RemoveTrailingSpaces(string[]? src)
    {
        if (src == null || src.Length == 0)
        {
            return [];
        }

        return src.Select(s =>
        {
            if (s is null)
            {
                return string.Empty;
            }

            return s.TrimEnd();
        }).ToArray();
    }

    /// <summary>
    /// Заменят спецсимвол перевода корректи html на пробел.
    /// </summary>
    /// <param name="src">Массив строк.</param>
    /// <returns>Замененые строки.</returns>
    internal string RemoveLineBreakEntity(string src)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            return string.Empty;
        }

        return src.Replace("&#10;", " ").Replace("\n", " ");
    }
}