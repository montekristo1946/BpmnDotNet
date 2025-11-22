namespace BpmnDotNet.Arm.Core.UiDomain.Services;

using BpmnDotNet.Arm.Core.Common;
using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.UiDomain.Abstractions;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Common.Entities;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
internal class PlanePanelHandler : IPlanePanelHandler
{
    private readonly ILogger<PlanePanelHandler> _logger;
    private readonly IElasticClient _elasticClient;
    private readonly ISvgConstructor _svgConstructor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanePanelHandler"/> class.
    /// </summary>
    /// <param name="logger">logger.</param>
    /// <param name="elasticClient">elasticClient.</param>
    /// <param name="svgConstructor">ISvgConstructor.</param>
    public PlanePanelHandler(
        ILogger<PlanePanelHandler> logger,
        IElasticClient elasticClient,
        ISvgConstructor svgConstructor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        _svgConstructor = svgConstructor ?? throw new ArgumentNullException(nameof(svgConstructor));
    }

    /// <inheritdoc />
    public async Task<string> GetPlane(string idBpmnProcess, SizeWindows sizeWindows)
    {
        var plane = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(idBpmnProcess) ?? new BpmnPlane();
        var descriptors = await GetDescriptor(plane);
        var svg = await _svgConstructor.CreatePlaneAsync(plane, [], sizeWindows, descriptors);

        return svg;
    }

    /// <inheritdoc/>
    public async Task<string> GetColorPlane(string idUpdateNodeJobStatus, SizeWindows sizeWindows)
    {
        var historyNodeState = await _elasticClient.GetDataFromIdAsync<HistoryNodeState>(
            idUpdateNodeJobStatus,
            [nameof(HistoryNodeState.ArrayMessageErrors)]) ?? new HistoryNodeState();

        var plane = await _elasticClient.GetDataFromIdAsync<BpmnPlane>(historyNodeState.IdBpmnProcess) ??
                    new BpmnPlane();
        var descriptors = await GetDescriptor(plane);
        var svg = await _svgConstructor.CreatePlaneAsync(plane, historyNodeState.NodeStaus, sizeWindows, descriptors);

        return svg;
    }

    /// <inheritdoc/>
    public double SetScrollValue(double? deltaY, double scaleInput)
    {
        const double maxScale = 6F;
        const double minScale = 0.5F;
        const double stepScale = 0.2f;

        var scale = scaleInput;

        if (deltaY < 0)
        {
            scale *= 1.0f + stepScale;
        }

        if (deltaY > 0)
        {
            scale *= 1.0f - stepScale;
        }

        if (scale < minScale || scale > maxScale)
        {
            return scaleInput;
        }

        return scale;
    }

    /// <inheritdoc/>
    public PointT CalculateOffset(PointT pointStart, PointT pointEnd, PointT currentOffset, double scaleCurrent)
    {
        var coefValueMoved = 1 / scaleCurrent;
        currentOffset.X += (pointStart.X - pointEnd.X) * -1 * coefValueMoved;
        currentOffset.Y += (pointStart.Y - pointEnd.Y) * -1 * coefValueMoved;

        return currentOffset;
    }

    private async Task<DescriptionData[]> GetDescriptor(BpmnPlane plane)
    {
        var result = new List<DescriptionData>();
        foreach (var planeShape in plane.Shapes)
        {
            var data = await _elasticClient.GetDataFromIdAsync<DescriptionData>(planeShape.BpmnElement);
            if (data != null)
            {
                result.Add(data);
            }
        }

        return result.ToArray();
    }
}