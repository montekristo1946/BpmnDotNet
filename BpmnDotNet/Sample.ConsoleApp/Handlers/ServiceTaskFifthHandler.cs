using System.Globalization;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class ServiceTaskFifthHandler : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(ServiceTaskFifthHandler);

    private readonly ILogger<ServiceTaskFifthHandler> _logger;

    public ServiceTaskFifthHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ServiceTaskFifthHandler>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        var cont = context as ContextSubProcess;
        if (cont is null)
        {
            _logger.LogError("[ServiceTaskFifthHandler:AsyncJobHandler] context is null ");
            return;
        }

        _logger.LogDebug($"[ServiceTaskFifthHandler:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.ContextSubProcessValue}");


        cont.ContextSubProcessValue = "this text fill in ServiceTaskFifthHandler";

        await Task.Delay(1, ctsToken);
    }


}