using System.Globalization;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Messages;

namespace Sample.ConsoleApp.Handlers;

public class ReceiveTaskFirstHandle : IBpmnHandler
{
    private readonly ILogger<ReceiveTaskFirstHandle> _logger;

    public ReceiveTaskFirstHandle(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ReceiveTaskFirstHandle>();
    }

    public string TaskDefinitionId { get; init; } = nameof(ReceiveTaskFirstHandle);
    public string Description { get; init; } =   "Осмоторщик обязан тщательно проверять сцепные устройства вагонов перед отправкой.\nНельзя принимать вагоны с признаками подтекания топлива или смазочных материалов.\nВсе замечания по техническому состоянию следует немедленно докладывать старшему мастеру.\nОсмотр должен проводиться согласно установленному регламенту и требованиям безопасности.\nПрофессионализм и внимательность — ключевые качества осмоторщика вагонов";

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        var cont = context as IMessageReceiveTask;
        if (cont is null)
        {
            _logger.LogError("[ServiceTaskFirstHandler:AsyncJobHandler] context is null ");
            return;
        }

        var resGet = cont.ReceivedMessage.TryGetValue(typeof(MessageExampleFirst), out var messageExampleFirst);
        if (!resGet || messageExampleFirst is null)
            throw new OperationCanceledException("Fail try Get key messageExampleFirst");

        var message = (MessageExampleFirst)messageExampleFirst;

        _logger.LogDebug($"[ReceiveTaskFirstHandle:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} " +
                         $"{resGet} {message.Email}");

        await Task.Delay(1, ctsToken);
    }
}