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
    public Func<IContextBpmnProcess, CancellationToken, Task> Handler { get; init; } = null!;


    /// <inheritdoc/>
    public Task<IEnumerable<Token>> ExecuteAsync(IContextBpmnProcess contextBpmnProcess)
    {
        throw new NotImplementedException();
    }

}