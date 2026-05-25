using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.SvgDomain.Service;

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
}