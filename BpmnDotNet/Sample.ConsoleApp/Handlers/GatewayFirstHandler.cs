using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Handlers;

public class GatewayFirstHandler : IBpmnHandler
{
    private readonly ILogger<GatewayFirstHandler> _logger;

    public GatewayFirstHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<GatewayFirstHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(GatewayFirstHandler);

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug($"[GatewayFirstHandler:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} ");

        var conditionRoute = context as IExclusiveGateWay;

        if (conditionRoute is null)
            throw new OperationCanceledException("Fail try Add key ConditionRoute");

        conditionRoute.ConditionRoute.TryAdd(nameof(GatewayFirstHandler), "Flow_in_SendTaskFirstHandler");

        await Task.Delay(1, ctsToken);
    }
}