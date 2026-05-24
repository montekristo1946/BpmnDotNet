namespace BpmnDotNet.ElasticClientDomain;

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
}