using BpmnDotNet.Abstractions.Common;
using BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Context;

namespace BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Activity;

public class TestActivity : IBpmnHandler
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