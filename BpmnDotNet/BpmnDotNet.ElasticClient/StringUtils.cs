using System.Globalization;

namespace BpmnDotNet.ElasticClient;

public static class StringUtils
{
    public static string CreateIndexName(Type type)
    {
        
        var nameClass = type.Name;
        return nameClass.ToLower(CultureInfo.InvariantCulture);
    }
}