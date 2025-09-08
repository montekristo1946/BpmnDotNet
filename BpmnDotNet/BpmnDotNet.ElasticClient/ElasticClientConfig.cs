namespace BpmnDotNet.ElasticClient;

/// <summary>
///     Конфигурация для Elastic.
/// </summary>
public class ElasticClientConfig
{
    /// <summary>
    ///     Gets or sets url до базы данных.
    /// </summary>
    public string ConnectionString { get; set; } = "http://localhost:9200";
}