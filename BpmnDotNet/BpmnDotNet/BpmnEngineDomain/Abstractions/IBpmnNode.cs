namespace BpmnDotNet.BpmnEngineDomain.Abstractions;

using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Dto;

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
    /// Метод вызова блока для engine.
    /// </summary>
    /// <param name="contextBpmnProcess">IContextBpmnProcess.</param>
    /// <returns>Token следующего шага.</returns>
    public Task<IEnumerable<Token>> ExecuteAsync(IContextBpmnProcess contextBpmnProcess);

    /// <summary>
    /// Gets метод вызова реальзации пользователя.
    /// </summary>
    public Func<IContextBpmnProcess, CancellationToken, Task> Handler { get; init; }
}