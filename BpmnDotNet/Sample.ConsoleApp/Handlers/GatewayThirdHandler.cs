using System.Globalization;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Handlers;

public class GatewayThirdHandler : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(GatewayThirdHandler);

    private readonly ILogger<GatewayThirdHandler> _logger;

    public GatewayThirdHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<GatewayThirdHandler>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug($"[GatewayThirdHandler:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} ");

        await Task.Delay(1, ctsToken);
    }


}