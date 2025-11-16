using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class ServiceTaskThirdHandler : IBpmnHandler
{
    private readonly ILogger<ServiceTaskThirdHandler> _logger;

    public ServiceTaskThirdHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ServiceTaskThirdHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(ServiceTaskThirdHandler);
    public string Description { get; init; } =   "Service task third";

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        if (context is ContextData cont)
            _logger.LogDebug($"[ServiceTaskThirdHandler:AsyncJobHandler]  " +
                             $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.TestValue2}");

    
        await Task.Delay(1, ctsToken);
    }
}