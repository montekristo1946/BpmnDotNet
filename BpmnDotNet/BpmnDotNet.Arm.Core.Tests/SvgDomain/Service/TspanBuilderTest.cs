using BpmnDotNet.Arm.Core.SvgDomain.Service;

namespace BpmnDotNet.Arm.Core.Tests.SvgDomain.Service;

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
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"11\">Поиск</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"22\">саморегуляторов</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"33\">на результатах</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"44\">от МЛ</tspan>", result);
    }
    
    [Fact] 
    public void BuildSvg_CheckSpliteCountLine_CreateTspan()
    {
        _builder.AddChild("Репорт, Ошибка измерения");
        _builder.AddChild("МЛ");
        
        var result = _builder.BuildSvg();
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"11\">Репорт,</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"22\">Ошибка</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"33\">измерения</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"44\">МЛ</tspan>", result);
    }
    
    [Fact] 
    public void BuildSvg_CheckCase2_CreateTspan()
    {
        _builder.AddChild("Репорт, без нарушений.&#10;Alarm.None");
        _builder.AddChild("МЛ");
        
        var result = _builder.BuildSvg();
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"11\">Репорт,</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"22\">без</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"33\">нарушений.</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"44\">Alarm.None</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"55\">МЛ</tspan>", result);
    }
    
    [Fact] 
    public void BuildSvg_CheckCase3_CreateTspan()
    {
        _builder.AddChild("Репорт, без нарушений.\nAlarm.None");
        _builder.AddChild("МЛ");
        
        var result = _builder.BuildSvg();
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"11\">Репорт,</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"22\">без</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"33\">нарушений.</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"44\">Alarm.None</tspan>", result);
        Assert.Contains("<tspan id=\"\" x=\"0\" y=\"55\">МЛ</tspan>", result);
    }
    
    [Fact]
    public void SplitLinesFromWhiteSpace_ChekLargeName_splitName()
    {
        var name = "SplitLinesFromWhiteSpace_ChekLargeName_splitName";
        var split = _builder.RemapLines([name]);

        Assert.Single(split);
    }

    [Fact]
    public void SplitLinesFromLongLine_ChekLargeName_splitName()
    {
        var name = "SplitLinesFromLongLine_ChekLargeName_splitName";
        var split = _builder.SplitLinesFromLongLine([name]);

        Assert.Equal(4, split.Length);
    }

    [Fact]
    public void MergeAllChild_WithMultipleElements_ReturnsJoinedWithSpaces()
    {
        // Arrange
        var elements = new List<string> { "Hello", "World", "Test" };

        // Act
        var result = _builder.MergeAllChild(elements);

        // Assert
        Assert.Equal("Hello World Test", result);
    }
    
    [Fact]
    public void MergeAllChild_WithSingleElement_ReturnsSameStringWithoutSpaces()
    {
        // Arrange
        var elements = new List<string> { "SingleElement" };

        // Act
        var result = _builder.MergeAllChild(elements);

        // Assert
        Assert.Equal("SingleElement", result);
        Assert.DoesNotContain(" ", result);
    }
    
    [Fact]
    public void MergeAllChild_WithEmptyList_ReturnsEmptyString()
    {
        // Arrange
        var elements = new List<string>();

        // Act
        var result = _builder.MergeAllChild(elements);

        // Assert
        Assert.Equal(string.Empty, result);
    }
    
    [Fact]
    public void MergeAllChild_WithNullElement_IgnoreNullElement()
    {
        // Arrange
        var elements = new List<string> { "Valid", null!, "Valid2" };

        // Act
        var result = _builder.MergeAllChild(elements);

        // Assert
        Assert.Equal("Valid Valid2", result);
    }
    
    [Theory]
    [InlineData(new[] { "a", "b", "c", "d", "e", "f" }, true)]   // 6 > 5 -> true
    [InlineData(new[] { "a", "b", "c", "d", "e" }, false)]       // 5 == 5, макс 1 < 15 -> false
    [InlineData(new[] { "a", "b", "c", "d" }, false)]            // 4 < 5, макс 1 < 15 -> false
    [InlineData(new[] { "1234567890123456" }, true)]             // 1 < 5, но длина 16 > 15 -> true
    [InlineData(new[] { "123456789012345" }, false)]             // длина 15 == 15 -> false
    [InlineData(new[] { "short", "longwordmorethan15charshere" }, true)]  // длина 26 > 15 -> true
    [InlineData(new[] { "word1", "word2", "word3" }, false)]     // 3 < 5, макс 5 < 15 -> false
    public void CheckRemapLines_WithVariousInputs_ReturnsExpected(
        string[] splitWords, 
        bool expected)
    {
        // Act
        var result = _builder.CheckRemapLines(splitWords);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(new[] { "test ", "hello " }, new[] { "test", "hello" })]
    [InlineData(new[] { " ", "  ", "   " }, new[] { "", "", "" })]
    [InlineData(new[] { "no trailing", "also no" }, new[] { "no trailing", "also no" })]
    [InlineData(new[] { null, "text ", null }, new[] { "", "text", "" })]
    [InlineData(new[] { "multiple  ", "  spaces  " }, new[] { "multiple", "  spaces" })]
    public void RemoveTrailingSpaces_WithVariousInputs_ReturnsExpected(string[] input, string[] expected)
    {
        // Act
        var result = _builder.RemoveTrailingSpaces(input);

        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("нарушений.&#10;Ala", "нарушений. Ala")]
    [InlineData("Hello&#10;World", "Hello World")]
    [InlineData("Start&#10;Middle&#10;End", "Start Middle End")]
    [InlineData("&#10;Leading", " Leading")]
    [InlineData("Trailing&#10;", "Trailing ")]
    [InlineData("NoEntity", "NoEntity")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("\t", "")]
    [InlineData("\n", "")]
    [InlineData(" &#10; ", "   ")] // Пробел + &#10; + пробел = 2 пробела
    [InlineData("&#10;&#10;&#10;", "   ")] // Три сущности = три пробела
    [InlineData("Text&#10;", "Text ")]
    [InlineData("&#10;Text&#10;", " Text ")]
    public void RemoveLineBreakEntity_VariousInputs_ReturnsExpected(string input, string expected)
    {
        // Act
        var result = _builder.RemoveLineBreakEntity(input);

        // Assert
        Assert.Equal(expected, result);
    }
}