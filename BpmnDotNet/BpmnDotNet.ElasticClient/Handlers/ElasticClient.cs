using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.ElasticClient.Handlers;

public class ElasticClient : IElasticClient
{
    private readonly ILogger<IElasticClient> _logger;
    private readonly int _maxRetryCount;
    private readonly TimeSpan _retryDelay;
    private readonly ElasticsearchClientSettings _settings;
    private ElasticsearchClient? _client;

    public ElasticClient(
        ElasticClientConfig config,
        ILogger<IElasticClient> logger,
        int maxRetryCount = 5,
        TimeSpan? retryDelay = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRetryCount = maxRetryCount;
        _retryDelay = retryDelay ?? TimeSpan.FromSeconds(5);

        _settings = new ElasticsearchClientSettings(new Uri(config.ConnectionString))
            .DefaultMappingFor<HistoryNodeState>(m => m
                .IndexName(StringUtils.CreateIndexName(typeof(HistoryNodeState)))
                .IdProperty(d => d.Id)
            )
            .DefaultMappingFor<BpmnPlane>(m => m
                .IndexName(StringUtils.CreateIndexName(typeof(BpmnPlane)))
                .IdProperty(d => d.Id)
            );
    }


    public async Task<bool> SetDataAsync<T>(T historyNodeState)
    {
        try
        {
            var client = await GetClient();
            var response = await client.IndexAsync(historyNodeState);
            if (!response.IsValidResponse)
            {
                _logger.LogError($"[SetHistoryNodeState] Fail:{response.Result}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[SetDataAsync] Error SetData: {ex.Message}");
        }

        return false;
    }

    public async Task<T?> GetDataFromIdAsync<T>(string id)
    {
        try
        {
            var client = await GetClient();
            var index = StringUtils.CreateIndexName(typeof(T));
            var response = await client.GetAsync<T>(index, id);

            if (!response.IsValidResponse || !response.Found) return default;

            var document = response.Source;
            return document is null
                ? default
                : document;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GetDataFromIdAsync] Error getting Data: {ex.Message}");
        }

        return default;
    }


    public async Task<HistoryNodeState[]> GetLastDataAsync(int count, string valueFind)
    {
        try
        {
            var client = await GetClient();
            var field = nameof(HistoryNodeState.IdBpmnProcess).ToElasticsearchFieldName();
            var index = StringUtils.CreateIndexName(typeof(HistoryNodeState));

            var response = await client.SearchAsync<HistoryNodeState>(s => s
                .Indices(index)
                .Query(q => q
                    .Match(m => m
                        .Field(new Field(field))
                        .Query(valueFind)
                    )
                )
                .Sort(sort => sort
                    .Field(p => p.DateCreated, f => f.Order(SortOrder.Desc)))
                .Size(count)
            );

            if (!response.IsValidResponse) return [];

            var retArr = response.Hits.Select(p => p.Source)
                .Where(p => p is not null)
                .ToArray();

            return retArr!;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GetLastDataAsync] Error getting Data: {ex.Message}");
        }

        return [];
    }

    public async Task<HistoryNodeState[]> SearchWithPaginationAsync(
        string valueFind,
        int pageNumber,
        int pageSize)
    {
        try
        {
            var client = await GetClient();
            var index = StringUtils.CreateIndexName(typeof(HistoryNodeState));

            var from = (pageNumber - 1) * pageSize;
            var response = await client.SearchAsync<HistoryNodeState>(s => s
                .Indices(index)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.IdBpmnProcess)
                        .Query(valueFind)
                    )
                )
                .Sort(sort => sort
                    .Field(p => p.DateCreated, f => f.Order(SortOrder.Desc))
                )
                .From(from)
                .Size(pageSize)
            );

            var retArr = response.Hits.Select(p => p.Source)
                .Where(p => p is not null)
                .ToArray();

            return retArr!;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[SearchWithPaginationAsync] Error getting Pagination: {ex.Message}");
        }

        return [];
    }

    public async Task<long> GetHistoryNodeStateCountAsync(string valueFind)
    {
        try
        {
            var client = await GetClient();
            var index = StringUtils.CreateIndexName(typeof(HistoryNodeState));
            var response = await client.CountAsync<HistoryNodeState>(c => c
                .Indices(index)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.IdBpmnProcess) // Замените на ваше поле
                        .Query(valueFind)
                    )
                )
            );

            return response.Count;
            ;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GetHistoryNodeStateCountAsync] Error getting unique count: {ex.Message}");
        }

        return 0;
    }

    private async Task<ElasticsearchClient> GetClient()
    {
        if (_client == null || !Ping()) await Reconnect();

        return _client ?? throw new InvalidOperationException("Elasticsearch client is null");
    }

    private bool Ping()
    {
        try
        {
            var response = _client?.Ping();

            return response?.IsValidResponse ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch ping failed");
            return false;
        }
    }

    private async Task Reconnect()
    {
        var retryCount = 0;
        while (retryCount < _maxRetryCount)
        {
            try
            {
                _logger.LogInformation("Attempting to reconnect to Elasticsearch (attempt {RetryCount})",
                    retryCount + 1);

                // Создаем нового клиента
                _client = new ElasticsearchClient(_settings);

                if (Ping())
                {
                    _logger.LogInformation("Successfully reconnected to Elasticsearch");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnect attempt {RetryCount} failed", retryCount + 1);
            }

            retryCount++;
            await Task.Delay(_retryDelay);
        }

        throw new InvalidOperationException($"Failed to reconnect to Elasticsearch after {_maxRetryCount} attempts");
    }
}