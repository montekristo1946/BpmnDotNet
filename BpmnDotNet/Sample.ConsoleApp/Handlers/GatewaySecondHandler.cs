using System.Globalization;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Handlers;

public class GatewaySecondHandler : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(GatewaySecondHandler);

    private readonly ILogger<GatewaySecondHandler> _logger;

    public GatewaySecondHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<GatewaySecondHandler>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug($"[GatewaySecondHandler:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} ");

        await Task.Delay(1, ctsToken);
    }


}