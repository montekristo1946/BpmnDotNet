using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Handlers;

public class GatewaySecondHandler : IBpmnHandler
{
    private readonly ILogger<GatewaySecondHandler> _logger;

    public GatewaySecondHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<GatewaySecondHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(GatewaySecondHandler);

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug($"[GatewaySecondHandler:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} ");

        await Task.Delay(1, ctsToken);
    }
}