using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using Microsoft.Extensions.Logging;

namespace BpmnDotNetTests.Utils;

internal class ProcessModelBuilderTestHelperNode : IBpmnNode
{
    public string Id { get; init; }

    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; }

    public ProcessModelBuilderTestHelperNode(
        ILogger<ProcessModelBuilderTestHelperNode> logger,
        Func<IContextBpmnProcess, CancellationToken, Task> handlerAsync,
        string id)
    {
        Id = id;
        ActivityHandlerAsync =  handlerAsync;
    }

    public Task<BpmnNodeResult> ExecuteAsync(
        ProcessModel processModel,
        IContextBpmnProcess context, ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ConcurrentDictionary<string, string> errorRegistry, CancellationToken cancellationToken)
    {
        return Task.FromResult(new BpmnNodeResult());
    }
}