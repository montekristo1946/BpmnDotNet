using System.Globalization;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.ClientDomain.Abstractions;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp.Context;

namespace Sample.ConsoleApp.Handlers;

internal class SubProcessFirstHandler : IBpmnHandler
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
    public string Description { get; init; } =   "Sub-process first handler";

    public async Task ActivityHandlerAsync(IContextBpmnProcess context, CancellationToken ctsToken)
    {
        _logger.LogDebug("[SubProcessFirstHandler:AsyncJobHandler] SubProcessFirstHandler run ");


        var contextSubProcess = CreateContextSubProcess();
        var taskNode = await _bpmnClient.StartNewProcessAsync(contextSubProcess, ctsToken);
        
        await taskNode.ProcessTask;

        await Task.Delay(1, ctsToken);

        var random = new Random();
        var randomState = random.Next(0, 10);
        if (randomState > 5)
        {
            // throw new Exception("Test exception!");
        }
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