namespace BpmnDotNet.ElasticClient.Handlers;

using System.Reflection;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class ElasticClient : IElasticClient
{
    private readonly ILogger<IElasticClient> _logger;
    private readonly int _maxRetryCount;
    private readonly TimeSpan _retryDelay;
    private readonly ElasticsearchClientSettings _settings;
    private ElasticsearchClient? _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticClient"/> class.
    /// </summary>
    /// <param name="config">ElasticClientConfig.</param>
    /// <param name="logger">ILogger.</param>
    /// <param name="maxRetryCount">maxRetryCount.</param>
    /// <param name="retryDelay">retryDelay.</param>
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
                .IdProperty(d => d.Id))
            .DefaultMappingFor<BpmnPlane>(m => m
                .IndexName(StringUtils.CreateIndexName(typeof(BpmnPlane)))
                .IdProperty(d => d.Id))
            .DefaultMappingFor<DescriptionData>(m => m
                .IndexName(StringUtils.CreateIndexName(typeof(DescriptionData)))
                .IdProperty(d => d.Id))
            ;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<T?> GetDataFromIdAsync<T>(string id, string[]? sourceExcludes)
    {
        try
        {
            var excludes = Array.Empty<string>();
            if (sourceExcludes is not null)
            {
                excludes = sourceExcludes.Select(p => p.ToElasticsearchFieldName()).ToArray();
            }

            var client = await GetClient();
            var index = StringUtils.CreateIndexName(typeof(T));
            var response = await client
                .GetAsync<T>(index, id, g => g
                    .SourceExcludes(excludes));

            if (!response.IsValidResponse || !response.Found)
            {
                return default;
            }

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

    /// <inheritdoc />
    public async Task<TIndex[]> GetAllFieldsAsync<TIndex>(
        string[] searchFields,
        int maxCountElements)
        where TIndex : class
    {
        try
        {
            var client = await GetClient();
            var index = StringUtils.CreateIndexName(typeof(TIndex));
            var fieldList = searchFields.Select(f => new Field(f.ToElasticsearchFieldName())).ToArray();

            var response = await client.SearchAsync<TIndex>(s => s
                .Indices(index)
                .Size(maxCountElements)
                .Source(src => src.Filter(f => f.Includes(fieldList)))
                .Query(q => q.MatchAll()));

            if (!response.IsValidResponse)
            {
                return [];
            }

            var retArr = response.IsValidResponse
                ? response.Hits.Select(h => h.Source).OfType<TIndex>().ToArray()
                : [];

            return retArr;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GetAllFields] Error getting Data: {ex.Message}");
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<int> GetCountHistoryNodeStateAsync(
        string idBpmnProcess,
        string[] processStatus,
        int sizeSample = 10000)
    {
        var fieldsValue = GetFields(processStatus);

        try
        {
            var client = await GetClient();

            var searchRequest = await client.SearchAsync<HistoryNodeState>(s => s
                .Size(sizeSample)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t
                                .Field(f => f.IdBpmnProcess.Suffix("keyword"))
                                .Value(idBpmnProcess)),
                            m => m.Terms(t => t
                                .Field(f => f.ProcessStatus.Suffix("keyword"))
                                .Terms(teams => teams.Value(fieldsValue))))))
                .Sort(s => s
                    .Field(f => f
                        .Field("dateCreated")
                        .Order(SortOrder.Desc)))
                .Source(src => src
                    .Filter(f => f
                        .Includes(new Field(nameof(HistoryNodeState.Id).ToElasticsearchFieldName())))));

            var retValue = searchRequest.Hits.Count;
            return retValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCountHistoryNodeState failed");
        }

        return 0;
    }

    /// <inheritdoc />
    public async Task<HistoryNodeState[]> GetHistoryNodeStateAsync(
        string idBpmnProcess,
        int from,
        int size,
        string[] processStatus)
    {
        var fieldsValue = GetFields(processStatus);

        try
        {
            var client = await GetClient();

            var searchRequest = await client.SearchAsync<HistoryNodeState>(s => s
                .From(from)
                .Size(size)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t
                                .Field(f => f.IdBpmnProcess.Suffix("keyword"))
                                .Value(idBpmnProcess)),
                            m => m.Terms(t => t
                                .Field(f => f.ProcessStatus.Suffix("keyword"))
                                .Terms(teams => teams.Value(fieldsValue))))))
                .Sort(s => s
                    .Field(f => f
                        .Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName())
                        .Order(SortOrder.Desc)))
                .Source(src => src
                    .Filter(f => f
                        .Includes(new Field[]
                        {
                            new Field(nameof(HistoryNodeState.Id).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.IdBpmnProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.TokenProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateLastModified).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.ProcessStatus).ToElasticsearchFieldName()),
                        }))));

            var retArr = searchRequest?.Hits
                ?.Select(p => p.Source)
                ?.Where(p => p is not null)
                ?.ToArray() ?? [];

            return retArr!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetHistoryNodeStateAsync failed");
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<HistoryNodeState[]> GetHistoryNodeFromTokenMaskAsync(
        string idBpmnProcess,
        string mask,
        int sizeSample = 100)
    {
        try
        {
            var client = await GetClient();
            var searchRequest = await client.SearchAsync<HistoryNodeState>(s => s
                .Size(sizeSample)
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.Term(t => t
                                .Field(f => f.IdBpmnProcess.Suffix("keyword"))
                                .Value(idBpmnProcess)),
                            m => m.Wildcard(t => t
                                .Field(f => f.TokenProcess.Suffix("keyword"))
                                .Value(mask)
                                .CaseInsensitive(true)))))
                .Sort(s => s
                    .Field(f => f
                        .Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName())
                        .Order(SortOrder.Desc)))
                .Source(src => src
                    .Filter(f => f
                        .Includes(new Field[]
                        {
                            new Field(nameof(HistoryNodeState.Id).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.IdBpmnProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.TokenProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateLastModified).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.ProcessStatus).ToElasticsearchFieldName()),
                        }))));

            var retArr = searchRequest?.Hits
                ?.Select(p => p.Source)
                ?.Where(p => p is not null)
                ?.ToArray() ?? [];

            return retArr!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetHistoryNodeFromTokenMaskAsync failed");
        }

        return [];
    }

    private async Task<ElasticsearchClient> GetClient()
    {
        if (_client == null || !Ping())
        {
            await Reconnect();
        }

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
                _client = new ElasticsearchClient(_settings);

                if (Ping())
                {
                    _logger.LogInformation("Successfully reconnected to Elasticsearch");
                    return;
                }

                _logger.LogError(
                    "Attempting to reconnect to Elasticsearch (attempt {RetryCount})",
                    retryCount + 1);
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

    private FieldValue[] GetFields(string[]? processStatus)
    {
        var allFields = new FieldValue[]
        {
            nameof(ProcessStatus.None),
            nameof(ProcessStatus.Works),
            nameof(ProcessStatus.Completed),
            nameof(ProcessStatus.Error),
        };
        var fieldsValue = processStatus?.Select(p =>
        {
            FieldValue f = p;
            return f;
        }).ToArray() ?? allFields;

        return fieldsValue;
    }
}