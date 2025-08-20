using System.Globalization;
using System.Net.Mail;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class ServiceTaskSecondHandler : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(ServiceTaskSecondHandler);

    private readonly ILogger<ServiceTaskSecondHandler> _logger;

    public ServiceTaskSecondHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ServiceTaskSecondHandler>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        if (context is ContextData cont)
        {
            _logger.LogDebug($"[ServiceTaskSecondHandler:AsyncJobHandler]  " +
                             $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.TestValue2}");
        }

        await Task.Delay(1, ctsToken);
    }
}