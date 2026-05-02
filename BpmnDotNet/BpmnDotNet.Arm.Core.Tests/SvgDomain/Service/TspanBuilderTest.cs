using BpmnDotNet.Arm.Core.SvgDomain.Service;

namespace BpmnDotNet.Arm.Core.Tests.Service;

public class TspanBuilderTest
{
    private readonly TspanBuilder _builder;

    public TspanBuilderTest()
    {
        _builder = new TspanBuilder();
    }
    
    [Fact] public void BuildSvg_FullPass_CreateTspan()
    {
        var text = "Поиск саморегуляторов на результатах от МЛ";
        _builder.AddChild(text);
        
        var result = _builder.BuildSvg();
        var normalizedResult = result.TrimEnd();
        Assert.Equal(182,normalizedResult.Length);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"11\">Поиск  </tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"22\">саморегуляторов </tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"33\">на результатах  </tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"44\">от МЛ  </tspan>", result);
    }
    
    [Fact]
    public void SplitLinesFromWhiteSpace_ChekLargeName_splitName()
    {
        var name = "SplitLinesFromWhiteSpace_ChekLargeName_splitName";
        var split = _builder.SplitLinesFromWhiteSpace(name);

        Assert.Single(split);
    }

    [Fact]
    public void SplitLinesFromLongLine_ChekLargeName_splitName()
    {
        var name = "SplitLinesFromLongLine_ChekLargeName_splitName";
        var split = _builder.SplitLinesFromLongLine([name]);

        Assert.Equal(4, split.Length);
    }
}