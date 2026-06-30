namespace BpmnDotNet.BpmnEngineDomain.Abstractions;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.Dto;

/// <summary>
/// Строитель ProcessModel.
/// </summary>
internal interface IProcessModelBuilder
{
    /// <summary>
    /// Построить BpmnProcessDto.
    /// </summary>
    /// <param name="bpmnProcessDto">BpmnProcessDto.</param>
    /// <param name="handlers">Func handlers.</param>
    /// <returns>ProcessModel.</returns>
    public ProcessModel Build(
        BpmnProcessDto bpmnProcessDto,
        ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> handlers);
}