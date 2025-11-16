using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class ServiceTaskFourthHandler : IBpmnHandler
{
    private readonly ILogger<ServiceTaskFourthHandler> _logger;

    public ServiceTaskFourthHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ServiceTaskFourthHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(ServiceTaskFourthHandler);
    public string Description { get; init; } =   "Service task fourth handler";

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        if (context is ContextData cont)
            _logger.LogDebug($"[ServiceTaskFourthHandler:AsyncJobHandler]  " +
                             $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.TestValue2}");

        await Task.Delay(1, ctsToken);
    }
}