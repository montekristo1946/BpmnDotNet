using System.Runtime.CompilerServices;
using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

[assembly: InternalsVisibleTo("BpmnDotNet.Arm.Core.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Arm.Core.DiagramBuilder;
public class TspanBuilder : IBpmnBuild<TspanBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private int _symbolInOneLine = 15;
    private const int FontSize = 11;
    private int _paddingY = 0;
    private int _paddingX = 0;


    public string Build()
    {
        var allLines = _childElements.SelectMany(SplitLinesFromWhiteSpace).ToArray();
        allLines = SplitLinesFromLongLine(allLines);

        for (var i = 0; i < allLines.Length; i++)
        {
            var body = allLines[i];
            var y = i * FontSize + FontSize + _paddingY;
            var x = _paddingX;
            var hider = $"<tspan x=\"{x}\" y=\"{y}\">";
            var footer = " </tspan>";
            _svgStorage.Append(hider);
            _svgStorage.Append(body);
            _svgStorage.Append(footer);
        }

        return _svgStorage.ToString();
    }

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

    internal string[] SplitLinesFromWhiteSpace(string input)
    {
        var arrWord = input.Split(' ');
        var segments = arrWord.Aggregate(new List<StringBuilder> { new() },
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

    public TspanBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

    public TspanBuilder AddMaxLenLine(int len)
    {
        _symbolInOneLine = len;
        return this;
    }

    public TspanBuilder AddPaddingY(int value)
    {
        _paddingY = value;
        return this;
    }

    public TspanBuilder AddPaddingX(int value)
    {
        _paddingX = value;
        return this;
    }
}