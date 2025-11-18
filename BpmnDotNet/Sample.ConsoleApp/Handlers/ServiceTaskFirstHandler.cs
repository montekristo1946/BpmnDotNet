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
    public string Description { get; init; } =   "Осмоторщик должен проверять пломбы на вагонах перед отправкой.\nЗапрещается допускать к эксплуатации вагоны с заметными повреждениями.\nРегулярно вести журнал осмотров и фиксировать все замечания.\nВнимательно осматривать дверные механизмы и системы безопасности.\nСоблюдать правила техники безопасности при работе с подвижным составом.";

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        if (context is ContextData cont)
            _logger.LogDebug($"[ServiceTaskFirstHandler:AsyncJobHandler]  " +
                             $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.TestValue2}");


        await Task.Delay(1, ctsToken);
    }
}