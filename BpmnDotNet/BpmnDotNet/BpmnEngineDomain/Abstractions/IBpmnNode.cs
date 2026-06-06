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
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>Token следующего шага.</returns>
    public Task<BpmnNodeResult> ExecuteAsync(IContextBpmnProcess contextBpmnProcess, CancellationToken cancellationToken);

    /// <summary>
    /// Gets метод вызова реализации пользователя.
    /// </summary>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; }
}