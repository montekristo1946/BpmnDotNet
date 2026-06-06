namespace BpmnDotNet.BpmnEngineDomain.Activity;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// EndEvent Activity.
/// </summary>
internal class EndEvent : IBpmnNode
{
    /// <inheritdoc/>
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; } = null!;


    /// <inheritdoc/>
    public Task<BpmnNodeResult> ExecuteAsync(IContextBpmnProcess contextBpmnProcess, CancellationToken cancellationToken)
    {
        return Task.FromResult(new BpmnNodeResult());
    }
}