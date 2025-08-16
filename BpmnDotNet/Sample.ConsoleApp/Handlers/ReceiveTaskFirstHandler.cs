using System.Globalization;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;
using Sample.ConsoleApp.Messages;

namespace Sample.ConsoleApp.Handlers;

public class ReceiveTaskFirstHandle : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = nameof(ReceiveTaskFirstHandle);

    private readonly ILogger<ReceiveTaskFirstHandle> _logger;

    public ReceiveTaskFirstHandle(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ReceiveTaskFirstHandle>();
    }

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        var cont = context as IMessageReceiveTask;
        if (cont is null)
        {
            _logger.LogError($"[ServiceTaskFirstHandler:AsyncJobHandler] context is null ");
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