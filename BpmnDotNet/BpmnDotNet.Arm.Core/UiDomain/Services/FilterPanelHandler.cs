namespace BpmnDotNet.Arm.Core.UiDomain.Services;

using BpmnDotNet.Arm.Core.UiDomain.Abstractions;
using BpmnDotNet.Arm.Core.UiDomain.Dto;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Entities;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class FilterPanelHandler : IFilterPanelHandler
{
    private readonly ILogger<FilterPanelHandler> _logger;
    private readonly IElasticClient _elasticClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterPanelHandler"/> class.
    /// </summary>
    /// <param name="logger">logger.</param>
    /// <param name="elasticClient">elasticClient.</param>
    public FilterPanelHandler(ILogger<FilterPanelHandler> logger, IElasticClient elasticClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    /// <inheritdoc />
    public async Task<ProcessDataFilterPanel[]> GetAllProcessIdAsync()
    {
        var searchFields = new[] { nameof(BpmnPlane.IdBpmnProcess), nameof(BpmnPlane.Name) };
        var bpmnPlanes = await _elasticClient.GetAllFieldsAsync<BpmnPlane>(searchFields, 100);

        var retArray = bpmnPlanes.Select(p => new ProcessDataFilterPanel()
        {
            IdBpmnProcess = p.IdBpmnProcess,
            NameBpmnProcess = p.Name,
        }).ToArray();
        return retArray;
    }
}