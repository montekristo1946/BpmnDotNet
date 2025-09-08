namespace BpmnDotNet.ElasticClient.Handlers;

using System.Globalization;

/// <summary>
/// Утилиты Elastic.
/// </summary>
internal static class StringUtils
{
    /// <summary>
    /// CreateIndexName.
    /// </summary>
    /// <param name="type">Type.</param>
    /// <returns>string.</returns>
    public static string CreateIndexName(Type type)
    {
        var nameClass = type.Name;
        return nameClass.ToLower(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// ToElasticsearchFieldName.
    /// </summary>
    /// <param name="propertyName">propertyName.</param>
    /// <returns>string.</returns>
    public static string ToElasticsearchFieldName(this string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return propertyName;
        }

        // Первый символ в нижний регистр
        return char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
    }
}