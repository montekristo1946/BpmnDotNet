using System.Text;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;

namespace BpmnDotNet.Arm.Core.DiagramBuilder;

public class LabelBuilder: IBpmnBuild<LabelBuilder>
{
 
    private readonly StringBuilder _svgStorage = new();
  
    private int _xElement;
    private int _yElement;
    private readonly List<string> _childElements = new();
    private string _id = string.Empty;

    public string Build()
    {
        var hider = $"<g data-element-id=\"{_id}\" style=\"display: block;\" transform=\"matrix(1 0 0 1 {_xElement} {_yElement})\">";
        
        var footer = "</g>";
        
        _svgStorage.AppendLine(hider);
        _childElements.ForEach(p => _svgStorage.AppendLine(p));
        _svgStorage.AppendLine(footer);

        return _svgStorage.ToString();
    }

    public LabelBuilder AddChild(string childElement)
    {
        _childElements.Add(childElement);
        return this;
    }

  
    public LabelBuilder AddPosition(int x, int y)
    {
        _xElement = x;
        _yElement = y;
        return this;
    }
    
    public LabelBuilder AddId(string id)
    {
        _id = id;
        return this;
    }
    

}