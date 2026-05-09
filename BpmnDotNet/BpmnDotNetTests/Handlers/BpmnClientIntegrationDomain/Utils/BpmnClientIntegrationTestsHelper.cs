using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Context;

namespace BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Utils;

internal class TestActivityInstance1 : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = "TestActivity";
    public string Description { get; init; } =  "Competed TestActivity";
    public Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken = default)
    {
        var conditionRoute = context as ContextData ?? throw new InvalidOperationException();

        conditionRoute.TestValue = "Competed TestActivity";
        return Task.CompletedTask;
    }
}

internal class TestActivityInstance2 : IBpmnHandler
{
    public string TaskDefinitionId { get; init; } = "TestActivity";
    public string Description { get; init; } =  "Competed TestActivity";
    public Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken = default)
    {
        var conditionRoute = context as ContextData ?? throw new InvalidOperationException();

        conditionRoute.TestValue = "Competed TestActivity";
        return Task.CompletedTask;
    }
}