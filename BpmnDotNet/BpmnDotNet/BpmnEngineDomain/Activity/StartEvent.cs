namespace BpmnDotNet.BpmnEngineDomain.Activity;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// StartEvent Activity.
/// </summary>
internal class StartEvent : IBpmnNode
{
    /// <inheritdoc/>
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public Func<IContextBpmnProcess, CancellationToken, Task> Handler { get; init; } = null!;


    /// <inheritdoc/>
    public Task<IEnumerable<Token>> ExecuteAsync(IContextBpmnProcess contextBpmnProcess, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

}