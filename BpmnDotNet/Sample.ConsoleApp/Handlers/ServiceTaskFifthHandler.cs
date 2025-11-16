using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class ServiceTaskFifthHandler : IBpmnHandler
{
    private readonly ILogger<ServiceTaskFifthHandler> _logger;

    public ServiceTaskFifthHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ServiceTaskFifthHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(ServiceTaskFifthHandler);
    public string Description { get; init; } =  "Осмоторщик вагонов обязан тщательно проверять целостность всех узлов и деталей.\nПри выявлении повреждений или неисправностей немедленно информировать начальство.\nСледить за правильностью крепления тормозных систем и состояние ходовой части.\nОбеспечивать своевременное оформление актов осмотра и проверок.\nБезопасность и исправность вагонов — главная забота осмоторщика.";

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