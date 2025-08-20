using System.Globalization;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class ServiceTaskFirstHandler : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(ServiceTaskFirstHandler);

    private readonly ILogger<ServiceTaskFirstHandler> _logger;

    public ServiceTaskFirstHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ServiceTaskFirstHandler>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        if (context is ContextData cont)
        {
            _logger.LogDebug($"[ServiceTaskFirstHandler:AsyncJobHandler]  " +
                             $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.TestValue2}");
        }




        await Task.Delay(1, ctsToken);
    }
}