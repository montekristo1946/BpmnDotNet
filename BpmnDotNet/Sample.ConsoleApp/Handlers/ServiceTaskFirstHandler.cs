using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class ServiceTaskFirstHandler : IBpmnHandler
{
    private readonly ILogger<ServiceTaskFirstHandler> _logger;

    public ServiceTaskFirstHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ServiceTaskFirstHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(ServiceTaskFirstHandler);
    public string Description { get; init; } =   "Service task first handler";

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        if (context is ContextData cont)
            _logger.LogDebug($"[ServiceTaskFirstHandler:AsyncJobHandler]  " +
                             $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.TestValue2}");


        await Task.Delay(1, ctsToken);
    }
}