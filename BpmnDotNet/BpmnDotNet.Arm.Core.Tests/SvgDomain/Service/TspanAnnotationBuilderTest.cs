using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.SvgDomain.Service;
using NSubstitute;

namespace BpmnDotNet.Arm.Core.Tests.SvgDomain.Service;

public class TspanAnnotationBuilderTest
{
    [Fact]
    public void BuildSvg_FullPass_CreateTspan()
    {
        var text = "1. Описание блока длинный текст\n\n3. Выше пробел...\n4. Строка номер четы описания бесконечно длинное\n5. Сочинение пушкина в 10 томах, а также сочинение Толстого в 15 томах";
        var tspan = IBpmnBuild<TspanAnnotationBuilder>
            .Create()
            .AddChild(text)
            .AddWidthBlock(190)
            .BuildSvg();
        
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"12\">1. Описание блока длинный</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"24\">текст</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"36\"></tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"48\">3. Выше пробел...</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"60\">4. Строка номер четы</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"72\">описания бесконечно</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"84\">длинное</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"96\">5. Сочинение пушкина в 10</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"108\">томах, а также сочинение</tspan>", tspan);
        Assert.Contains("<tspan id=\"\" x=\"7\" y=\"120\">Толстого в 15 томах</tspan>", tspan);
    }
    
    [Theory]
    [InlineData(new[] { "Short line" }, 10, new string[] { "Short line" })]
    [InlineData(new[] { "This is a very long line that needs splitting" }, 10, 
        new[] { "This is a", "very long", "line that", "needs", "splitting" })]
    [InlineData(new[] { "WordWithoutSpaces" }, 5, 
        new[] { "WordW", "ithou", "tSpac", "es" })]
    [InlineData(new[] { "" }, 10, new string[] { "" })]
    [InlineData(new[] { "   " }, 10, new string[] { "   " })]
    [InlineData(new string[] { null! }, 10, new string[] { null! })]
    [InlineData(new[] { "One two three", "Short", "Very long line for split" }, 8,
        new[] { "One two", "three", "Short", "Very", "long", "line for", "split" })]
    [InlineData(new[] { "A B C D E F G H I J K" }, 5,
        new[] { "A B C", "D E F", "G H I", "J K" })]
    [InlineData(new[] { "Exactly ten", "1234567890" }, 10,
        new[] { "Exactly","ten", "1234567890" })]
    public void SplitLongLine_VariousInputs_ReturnsExpectedResult(
        string[] lines, int maxSymbol, string[] expected)
    {
        // Arrange
        var service = new TspanAnnotationBuilder();
        
        // Act
        var result = service.SplitLongLine(lines, maxSymbol);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(int.MinValue)]
    public void SplitLongLine_InvalidMaxSymbol_ThrowsArgumentException(int maxSymbol)
    {
        // Arrange
        var lines = new[] { "Some text" };
        var service = new TspanAnnotationBuilder();
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.SplitLongLine(lines, maxSymbol));
    }
}