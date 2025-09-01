using System.Globalization;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

public class SubProcessFirstHandler : IBpmnHandler
{
    private readonly IBpmnClient _bpmnClient;

    private readonly ILogger<SubProcessFirstHandler> _logger;

    public SubProcessFirstHandler(ILoggerFactory loggerFactory, IBpmnClient bpmnClient)
    {
        _bpmnClient = bpmnClient ?? throw new ArgumentNullException(nameof(bpmnClient));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<SubProcessFirstHandler>();
    }

    public string TaskDefinitionId { get; init; } = nameof(SubProcessFirstHandler);

    public async Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug("[SubProcessFirstHandler:AsyncJobHandler] SubProcessFirstHandler run ");
        var cont = context as ContextData;


        var timeout = TimeSpan.FromMinutes(10);
        var contextSubProcess = CreateContextSubProcess();
        var taskNode = _bpmnClient.StartNewProcess(contextSubProcess, timeout);

        await taskNode.ProcessTask;

        throw new Exception("Тестовое сообщение ошибки");
        
        _logger.LogDebug($"[SubProcessFirstHandler:AsyncJobHandler]  " +
                         $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)} {contextSubProcess.ContextSubProcessValue}");
    }

    private ContextSubProcess CreateContextSubProcess()
    {
        return new ContextSubProcess
        {
            ContextSubProcessValue = "text sub process"
        };
    }
}