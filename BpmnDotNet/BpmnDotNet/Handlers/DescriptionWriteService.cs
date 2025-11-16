namespace BpmnDotNet.Handlers;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Entities;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
internal class DescriptionWriteService : IDescriptionWriteService
{
    private readonly IElasticClientSetDataAsync _elasticClient;
    private readonly ILogger<DescriptionWriteService> _logger;
    private ConcurrentDictionary<string, string> _dictionary = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionWriteService"/> class.
    /// </summary>
    /// <param name="elasticClient">Клиент Elastic.</param>
    /// <param name="logger">ILogger.</param>
    public DescriptionWriteService(IElasticClientSetDataAsync elasticClient, ILogger<DescriptionWriteService> logger)
    {
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void AddDescription(string taskDefinitionId, string description)
    {
        _dictionary.AddOrUpdate(
            taskDefinitionId,
            _ => description,
            (keyOld, oldMessage) =>
                description);
    }

    /// <inheritdoc />
    public async Task Commit()
    {
        var saveArr = _dictionary.Select(p => new DescriptionData()
        {
            TaskDefinitionId = p.Key,
            Description = p.Value,
        }).ToArray();

        foreach (var item in saveArr)
        {
            var checkSave = await _elasticClient.SetDataAsync(item);
            if (!checkSave)
            {
                _logger.LogError("Failed to update description, {TaskDefinitionId}", item.TaskDefinitionId);
            }
        }
    }

    /// <inheritdoc/>
    public Task Init()
    {
        _dictionary = new ConcurrentDictionary<string, string>();
        return Task.CompletedTask;
    }
}