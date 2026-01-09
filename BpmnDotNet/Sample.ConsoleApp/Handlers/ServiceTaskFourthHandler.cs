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
    public string Description { get; init; } =   "Осмоторщик обязан проверять тормозные системы на исправность перед каждым рейсом.\nВсе выявленные неисправности должны быть тщательно зафиксированы и немедленно исправлены.\nЗапрещается пропускать вагоны с поврежденными колесами или шинами.\nСледует соблюдать чистоту и порядок на рабочем месте во время осмотра.\nВнимательность и ответственность — залог безопасности подвижного состава";

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        if (context is ContextData cont)
            _logger.LogDebug($"[ServiceTaskFourthHandler:AsyncJobHandler]  " +
                             $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {cont.TestValue2}");

        var random = new Random();
        var randomState = random.Next(0, 10);
        if (randomState > 5)
        {
            throw new Exception("Test exception!");
        }
        
        await Task.Delay(1, ctsToken);
    }
}