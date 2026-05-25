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
}