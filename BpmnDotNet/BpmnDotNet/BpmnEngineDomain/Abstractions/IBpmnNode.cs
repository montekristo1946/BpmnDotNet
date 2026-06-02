using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Dto;

namespace BpmnDotNet.BpmnEngineDomain.Abstractions;

/// <summary>
/// Описывает базовый функционал.
/// </summary>
internal interface IBpmnNode
{
    /// <summary>
    /// Уникальный идентификатор.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Метод вызова.
    /// </summary>
    /// <param name="contextBpmnProcess">IContextBpmnProcess.</param>
    /// <returns>Token следующего шага.</returns>
    public Task<IEnumerable<Token>> ExecuteAsync(IContextBpmnProcess contextBpmnProcess);
}