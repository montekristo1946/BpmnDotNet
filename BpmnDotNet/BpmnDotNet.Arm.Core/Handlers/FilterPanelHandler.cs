using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.BPMNDiagram;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Arm.Core.Handlers;

public class FilterPanelHandler:IFilterPanelHandler
{
    private readonly ILogger<FilterPanelHandler> _logger;
    private readonly IElasticClient  _elasticClient;

    public FilterPanelHandler(ILogger<FilterPanelHandler> logger, IElasticClient elasticClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    public async Task<string[]> GetAllProcessId()
    {
       var bpmnPlanes = await _elasticClient.GetAllFields<BpmnPlane,string>(nameof(BpmnPlane.IdBpmnProcess),100);

       return bpmnPlanes ?? [];
    }
}