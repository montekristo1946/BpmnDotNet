using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class TspanBuilder: IBpmnBuild<TspanBuilder>
{
    private readonly StringBuilder _svgStorage = new();
    private readonly List<string> _childElements = new();
    private const int SymbolInOneLine = 12;
    private const int FontSize = 11;
    
    public string Build()
    {
        var allLines = _childElements.SelectMany(SpliteLines).ToArray();

        for (var i = 0; i < allLines.Length; i++)
        {
            var body = allLines[i];
            var y = i * FontSize+FontSize;
            var x = 0;
            var hider = $"<tspan x=\"{x}\" y=\"{y}\">";
            var footer = " </tspan>";
            _svgStorage.Append(hider);
            _svgStorage.Append(body);
            _svgStorage.Append(footer);
        }
        return _svgStorage.ToString();
    }

    private string [] SpliteLines(string input)
    {
        var segments = Enumerable.Range(0, (int)Math.Ceiling(input.Length /(double)SymbolInOneLine))
            .Select(i => input.Substring(i * SymbolInOneLine, 
                Math.Min(SymbolInOneLine, input.Length - i * SymbolInOneLine)));
        
        return segments.ToArray();
    }

    public TspanBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }
}