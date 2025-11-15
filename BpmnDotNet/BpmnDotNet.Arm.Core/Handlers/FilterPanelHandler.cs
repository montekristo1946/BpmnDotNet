using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Arm.Core.Handlers;

public class FilterPanelHandler : IFilterPanelHandler
{
    private readonly ILogger<FilterPanelHandler> _logger;
    private readonly IElasticClient _elasticClient;

    public FilterPanelHandler(ILogger<FilterPanelHandler> logger, IElasticClient elasticClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

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