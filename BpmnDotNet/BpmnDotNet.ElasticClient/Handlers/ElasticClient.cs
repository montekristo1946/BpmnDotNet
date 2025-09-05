using System.Reflection;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using BpmnDotNet.Common.Dto;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport.Extensions;
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
            var response = client.IndexAsync(historyNodeState).Result;
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
            var response = client
                .GetAsync<T>(index, id, g => g
                    .SourceExcludes(excludes))
                .Result;

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


    // public async Task<HistoryNodeState[]> GetLastDataAsync(int count, string valueFind)
    // {
    //     try
    //     {
    //         var client = await GetClient();
    //         var field = nameof(HistoryNodeState.IdBpmnProcess).ToElasticsearchFieldName();
    //         var index = StringUtils.CreateIndexName(typeof(HistoryNodeState));
    //
    //         var response =  client.SearchAsync<HistoryNodeState>(s => s
    //             .Indices(index)
    //             .Query(q => q
    //                 .Match(m => m
    //                     .Field(new Field(field))
    //                     .Query(valueFind)
    //                 )
    //             )
    //             .Sort(sort => sort
    //                 .Field(p => p.DateCreated, f => f.Order(SortOrder.Desc)))
    //             .Size(count)
    //         ).Result;
    //
    //         if (!response.IsValidResponse) return [];
    //
    //         var retArr = response.Hits.Select(p => p.Source)
    //             .Where(p => p is not null)
    //             .ToArray();
    //
    //         return retArr!;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError($"[GetLastDataAsync] Error getting Data: {ex.Message}");
    //     }
    //
    //     return [];
    // }

    // public async Task<HistoryNodeState[]> SearchWithPaginationAsync_(
    //     string valueFind,
    //     int pageNumber,
    //     int pageSize)
    // {
    //     try
    //     {
    //         var client = await GetClient();
    //         var index = StringUtils.CreateIndexName(typeof(HistoryNodeState));
    //
    //         var from = (pageNumber - 1) * pageSize;
    //         var response =  client.SearchAsync<HistoryNodeState>(s => s
    //             .Indices(index)
    //             .Query(q => q
    //                 .Match(m => m
    //                     .Field(f => f.IdBpmnProcess)
    //                     .Query(valueFind)
    //                 )
    //             )
    //             .Sort(sort => sort
    //                 .Field(p => p.DateCreated, f => f.Order(SortOrder.Desc))
    //             )
    //             .From(from)
    //             .Size(pageSize)
    //         ).Result;
    //
    //         var retArr = response.Hits.Select(p => p.Source)
    //             .Where(p => p is not null)
    //             .ToArray();
    //
    //         return retArr!;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError($"[SearchWithPaginationAsync] Error getting Pagination: {ex.Message}");
    //     }
    //
    //     return [];
    // }

    /* public async Task<long> GetHistoryNodeStateCountAsync(string valueFind)
     {
         try
         {
             var client = await GetClient();
             var index = StringUtils.CreateIndexName(typeof(HistoryNodeState));
             var response = await client.CountAsync<HistoryNodeState>(c => c
                 .Indices(index)
                 .Query(q => q
                     .Match(m => m
                         .Field(f => f.IdBpmnProcess)
                         .Query(valueFind)
                     )
                 )
             );

             return response.Count;
         }
         catch (Exception ex)
         {
             _logger.LogError($"[GetHistoryNodeStateCountAsync] Error getting unique count: {ex.Message}");
         }

         return 0;
     }*/

    public async Task<TField[]> GetAllFieldsAsync<TIndex, TField>(
        string nameField,
        int maxCountElements)
        where TIndex : class
    {
        try
        {
            var client = await GetClient();
            var index = StringUtils.CreateIndexName(typeof(TIndex));
            var field = nameField.ToElasticsearchFieldName();

            var response = client.SearchAsync<TIndex>(s => s
                .Indices(index)
                .Size(maxCountElements)
                .Source(src => src.Filter(f => f.Includes(new Field(field))))
                .Query(q => q.MatchAll())
            ).Result;

            if (!response.IsValidResponse)
            {
                return [];
            }

            var fieldValues = new List<TField>();
            var property = typeof(TIndex).GetProperty(nameField,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            foreach (var document in response.Documents)
            {
                if (property == null)
                    continue;

                var value = property.GetValue(document);
                if (value is TField typedValue)
                {
                    fieldValues.Add(typedValue);
                }
            }

            return fieldValues.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError($"[GetAllFields] Error getting Data: {ex.Message}");
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
                _logger.LogError("Attempting to reconnect to Elasticsearch (attempt {RetryCount})",
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

    // public async Task<int> GetAllGroupFromTokenAsync(string idActiveProcess)
    // {
    //     var countAnalized = 10000;
    //     try
    //     {
    //         var client = await GetClient();
    //         var keySort = "tokenProcess";
    //         var response = client.SearchAsync<HistoryNodeState>(s => s
    //             .Size(0)
    //             .Query(q => q
    //                 .Term(t => t
    //                     .Field(f => f.IdBpmnProcess.Suffix("keyword"))
    //                     .Value(idActiveProcess)
    //                 )
    //             )
    //             .Aggregations(aggs => aggs
    //                 .Add("unique_combinations", c =>
    //                     c.Composite(l => l
    //                         .Size(countAnalized)
    //                         .Sources(src =>
    //                             src.Add(keySort, t =>
    //                                 t.Terms(descriptor => descriptor
    //                                     .Field(f => f.TokenProcess.Suffix("keyword"))
    //                                 )
    //                             )
    //                         ))
    //                 )
    //             )
    //         ).Result;
    //
    //         if (!response.IsValidResponse || response.Aggregations is null)
    //         {
    //             return 0;
    //         }
    //
    //         var compositeAgg = response.Aggregations.GetComposite("unique_combinations");
    //
    //         return compositeAgg?.Buckets?.Count ?? 0;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "GetAllGroupFromToken failed");
    //     }
    //
    //     return 0;
    // }


    // public async Task<string[]> GetIdHistoryNodeStateAsync(string idActiveProcess, string afterKeyValue,
    //     int countLineOnePage)
    // {
    //     try
    //     {
    //         var client = await GetClient();
    //         var keySort = "tokenProcess";
    //
    //         var response = client.SearchAsync<HistoryNodeState>(s => s
    //             .Size(0)
    //             .Query(q => q
    //                 .Term(t => t
    //                     .Field(f => f.IdBpmnProcess.Suffix("keyword"))
    //                     .Value(idActiveProcess)
    //                 )
    //             )
    //             .Sort(sort => sort
    //                 .Field(f => f.DateCreated, so => so.Order(SortOrder.Desc)))
    //             .Aggregations(aggs => aggs
    //                 .Add("tokens", t => t
    //                     .Composite(c =>
    //                         c.Size(countLineOnePage)
    //                             .Sources(src =>
    //                                 src.Add(keySort, t =>
    //                                     t.Terms(descriptor => descriptor
    //                                         .Field(f => f.TokenProcess.Suffix("keyword"))
    //                                     )
    //                                 )
    //                             )
    //                             .After(a => a.Add(aftKey =>
    //                                 aftKey.TokenProcess, afterKeyValue))
    //                     )
    //                     .Aggregations(subAggs => subAggs
    //                         .Add("latest_doc", ld => ld
    //                             .TopHits(th => th
    //                                 .Size(1)
    //                                 .Sort(sort => sort
    //                                     .Field(f => f.DateLastModified, so => so.Order(SortOrder.Desc)))
    //                             ))
    //                     )
    //                 )
    //             )).Result;
    //
    //
    //         if (!response.IsValidResponse || response.Aggregations is null)
    //         {
    //             return [];
    //         }
    //
    //         var bucketsHistory = response.Aggregations.GetComposite("tokens")?.Buckets;
    //
    //         var retIds = bucketsHistory?.Select(bucket =>
    //         {
    //             var retId = string.Empty;
    //             var docks = bucket.Aggregations?.FirstOrDefault().Value;
    //             switch (docks)
    //             {
    //                 case null:
    //                     break;
    //                 case TopHitsAggregate compositeAgg:
    //                     retId = compositeAgg?.Hits?.Hits?.FirstOrDefault()?.Id ?? string.Empty;
    //                     break;
    //             }
    //
    //             return retId;
    //         })?.ToArray();
    //
    //         return retIds ?? [];
    //         // foreach (var bucket in bucketsHistory)
    //         // {
    //         //
    //         //     foreach (var docks  in bucket.Aggregations)
    //         //     {
    //         //
    //         //         // var valueDoc = docks.Value as TopHitsAggregate;
    //         //         
    //         //         if (docks.Value  is TopHitsAggregate compositeAgg)
    //         //         {
    //         //             // Работаем с composite агрегацией
    //         //             // foreach (var t in compositeAgg.Hits)
    //         //             // {
    //         //             //     // Обработка бакетов
    //         //             // }
    //         //             var t = compositeAgg.Hits.Hits.FirstOrDefault();
    //         //             Console.WriteLine();
    //         //         }
    //         //         Console.WriteLine();
    //         //    
    //         //     }
    //         //
    //         // }
    //         return [];
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "GetHistoryNodeStateAsync failed");
    //     }
    //
    //     return [];
    // }

    public async Task<int> GetCountHistoryNodeState(string idBpmnProcess, string[] processStatus = null,
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
                                .Terms(teams => teams.Value(fieldsValue)))
                        )
                    )
                )
                .Sort(s => s
                    .Field(f => f
                        .Field("dateCreated")
                        .Order(SortOrder.Desc)
                    )
                )
                .Source(src => src
                    .Filter(f => f
                        .Includes(new Field(nameof(HistoryNodeState.Id).ToElasticsearchFieldName()))
                    )
                ));

            var retValue = searchRequest.Hits.Count;
            return retValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCountHistoryNodeState failed");
        }

        return 0;
    }

    public async Task<HistoryNodeState[]> GetHistoryNodeStateAsync(string idBpmnProcess, int from, int size,
        string[] processStatus = null)
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
                                .Terms(teams => teams.Value(fieldsValue)))
                        )
                    )
                )
                .Sort(s => s
                    .Field(f => f
                        .Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName())
                        .Order(SortOrder.Desc)
                    )
                )
                .Source(src => src
                    .Filter(f => f
                        .Includes(new Field[]
                        {
                            new Field(nameof(HistoryNodeState.Id).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.IdBpmnProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.TokenProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateLastModified).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.ProcessStatus).ToElasticsearchFieldName())
                        })
                    )
                ));

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

    public async Task<HistoryNodeState[]> GetHistoryNodeFromTokenMaskAsync(string idBpmnProcess, string mask,
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
                                .CaseInsensitive(true)
                            )
                        )
                    )
                )
                .Sort(s => s
                    .Field(f => f
                        .Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName())
                        .Order(SortOrder.Desc)
                    )
                )
                .Source(src => src
                    .Filter(f => f
                        .Includes(new Field[]
                        {
                            new Field(nameof(HistoryNodeState.Id).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.IdBpmnProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.TokenProcess).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateCreated).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.DateLastModified).ToElasticsearchFieldName()),
                            new Field(nameof(HistoryNodeState.ProcessStatus).ToElasticsearchFieldName())
                        })
                    )
                )
            );

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

    private static FieldValue[] GetFields(string[]? processStatus)
    {
        var allFields = new FieldValue[]
        {
            nameof(ProcessStatus.None),
            nameof(ProcessStatus.Works),
            nameof(ProcessStatus.Completed),
            nameof(ProcessStatus.Error)
        };
        var fieldsValue = processStatus?.Select(p =>
        {
            FieldValue f = p;
            return f;
        }).ToArray() ?? allFields;

        return fieldsValue;
    }
}