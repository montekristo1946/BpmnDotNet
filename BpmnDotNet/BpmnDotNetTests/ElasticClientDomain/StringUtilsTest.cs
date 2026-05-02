using AutoFixture;
using BpmnDotNet.Dto;
using BpmnDotNet.ElasticClientDomain;

namespace BpmnDotNetTests.ElasticClientDomain;

public class StringUtilsTest
{
    
    [Theory]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(int), "int32")]
    [InlineData(typeof(BusinessProcessJobStatus), "businessprocessjobstatus")]
    [InlineData(typeof(HistoryNodeState), "historynodestate")]
    [InlineData(typeof(NodeJobStatus), "nodejobstatus")]
    [InlineData(typeof(ProcessStatus), "processstatus")]
    [InlineData(typeof(StatusType), "statustype")]
    public void CreateIndexName_ShouldReturnCorrectName(Type type, string expected)
    {
        // Act
        var result = StringUtils.CreateIndexName(type);
    
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Name", "name")]
    [InlineData("PropertyName", "propertyName")]
    [InlineData("UPPERCASE", "uPPERCASE")]
    [InlineData("lowercase", "lowercase")]
    [InlineData("alreadyLower", "alreadyLower")]
    [InlineData("XMLParser", "xMLParser")]
    [InlineData("HTMLDocument", "hTMLDocument")]
    [InlineData("a", "a")]
    [InlineData("A", "a")]        [InlineData("SingleWord", "singleWord")]
    [InlineData("VeryLongPropertyName", "veryLongPropertyName")]
    [InlineData("_underscoreStart", "_underscoreStart")]
    [InlineData("123StartWithNumber", "123StartWithNumber")]
    [InlineData("Special@Character", "special@Character")]
    [InlineData("CamelCaseWORD", "camelCaseWORD")]
    public void ToElasticsearchFieldName_ShouldConvertFirstCharToLower_WhenValidString(string input, string expected)
    {
        // Act
        var result = input.ToElasticsearchFieldName();
            
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ToElasticsearchFieldName_ShouldReturnOriginalString_WhenStringIsNullOrWhiteSpace(string input)
    {
        // Act
        var result = input.ToElasticsearchFieldName();
            
        // Assert
        Assert.Equal(input, result);
    }
}