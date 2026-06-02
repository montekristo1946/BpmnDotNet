namespace BpmnDotNet.BpmnEngineDomain;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <inheritdoc cref="StartProcessAsync" />
internal class BpmnEngine : IBpmnEngine, IDisposable
{
    /// <inheritdoc/>
    public Task<BusinessProcessJobStatusV2> StartProcessAsync(
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}