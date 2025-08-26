using System.Globalization;

namespace BpmnDotNet.ElasticClient.Handlers;

public static class StringUtils
{
    public static string CreateIndexName(Type type)
    {
        var nameClass = type.Name;
        return nameClass.ToLower(CultureInfo.InvariantCulture);
    }

    public static string ToElasticsearchFieldName(this string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return propertyName;

        // Первый символ в нижний регистр
        return char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
    }
}