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
    /// <param name="processModel"><inheritdoc cref="ProcessModel"/></param>
    /// <param name="contextBpmnProcess"><inheritdoc cref="IContextBpmnProcess"/>.</param>
    /// <param name="cancellationToken"><inheritdoc cref="CancellationToken"/>.</param>
    /// <returns><inheritdoc cref="BpmnNodeResult"/>.</returns>
    public Task<BpmnNodeResult> ExecuteAsync(
        ProcessModel processModel,
        IContextBpmnProcess contextBpmnProcess,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets метод вызова реализации пользователя.
    /// </summary>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; }
}