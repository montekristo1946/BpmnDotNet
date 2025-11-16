using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace Sample.ConsoleApp.Handlers;

public class SendTaskFirstHandler : IBpmnHandler
{
    private readonly ILogger<SendTaskFirstHandler> _logger;

    public SendTaskFirstHandler(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<SendTaskFirstHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(SendTaskFirstHandler);

    public string Description { get; init; } =
        "Отцепка вагонов производится только при наличии технической неисправности или нарушении условий перевозки.\n" +
        "Перед отцепкой необходимо оформить соответствующую документацию и уведомить ответственные службы для оперативного устранения причин.\n" +
        "Отцепленные вагоны подлежат направлению в вагонное депо для диагностики, ремонта или таможенных процедур в установленном порядке.";


    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug($"[SendTaskFirstHandler:AsyncJobHandler] " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}", context);
        await Task.Delay(1, ctsToken);
    }
}