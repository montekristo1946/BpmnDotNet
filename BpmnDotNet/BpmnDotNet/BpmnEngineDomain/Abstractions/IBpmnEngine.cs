namespace BpmnDotNet.BpmnEngineDomain.Abstractions;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Движок выполнения Bpmn.
/// </summary>
internal interface IBpmnEngine
{
    /// <summary>
    /// Запустить процесс.
    /// </summary>
    /// <param name="contextBpmnProcess">Context.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>BusinessProcessJobStatus.</returns>
    public Task<BusinessProcessJobStatusV2> StartProcessAsync(IContextBpmnProcess contextBpmnProcess, CancellationToken ct);
}