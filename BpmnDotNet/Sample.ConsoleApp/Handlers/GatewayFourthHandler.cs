using System.Globalization;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Interfases;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Handlers;

public class GatewayFourthHandler : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(GatewayFourthHandler);

    private readonly ILogger<GatewayFourthHandler> _logger;

    public GatewayFourthHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<GatewayFourthHandler>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug($"[GatewayFourthHandler:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} ");

        await Task.Delay(1, ctsToken);
    }


}