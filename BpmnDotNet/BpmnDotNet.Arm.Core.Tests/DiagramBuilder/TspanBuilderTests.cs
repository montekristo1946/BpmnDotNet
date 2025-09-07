using BpmnDotNet.Arm.Core.DiagramBuilder;

namespace BpmnDotNet.Arm.Core.Tests.DiagramBuilder;

public class TspanBuilderTests
{
    private readonly TspanBuilder _builder;

    public TspanBuilderTests()
    {
        _builder =  new TspanBuilder();
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