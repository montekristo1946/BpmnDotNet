namespace BpmnDotNet.ElasticClientDomain;

/// <summary>
///     Конфигурация для Elastic.
/// </summary>
public class ElasticClientConfig
{
    /// <summary>
    ///     Gets url до базы данных.
    /// </summary>
    public string ConnectionString { get; init; } = "http://localhost:9200";
}