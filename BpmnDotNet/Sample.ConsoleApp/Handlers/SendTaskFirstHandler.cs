using System.Globalization;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Handlers;

public class SendTaskFirstHandler : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(SendTaskFirstHandler);

    private readonly ILogger<SendTaskFirstHandler> _logger;

    public SendTaskFirstHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<SendTaskFirstHandler>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug($"[SendTaskFirstHandler:AsyncJobHandler] " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}", context);
        await Task.Delay(1, ctsToken);
    }


}