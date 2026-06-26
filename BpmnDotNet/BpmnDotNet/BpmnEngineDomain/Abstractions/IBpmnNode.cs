namespace BpmnDotNet.BpmnEngineDomain.Abstractions;

using System.Collections.Concurrent;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Описывает базовый функционал.
/// </summary>
internal interface IBpmnNode
{
    /// <summary>
    /// Gets уникальный идентификатор.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets метод вызова реализации пользователя.
    /// </summary>
    public Func<IContextBpmnProcess, CancellationToken, Task> ActivityHandlerAsync { get; init; }

    /// <summary>
    /// Метод вызова блока для engine.
    /// </summary>
    /// <param name="processModel"><inheritdoc cref="ProcessModel"/></param>
    /// <param name="context"><inheritdoc cref="IContextBpmnProcess"/>.</param>
    /// <param name="nodeStateRegistry">Реестр состояние узлов.</param>
    /// <param name="errorRegistry">Реестр сообщение ошибок.</param>
    /// <param name="cancellationToken"><inheritdoc cref="CancellationToken"/>.</param>
    /// <returns><inheritdoc cref="BpmnNodeResult"/>.</returns>
    public Task<BpmnNodeResult> ExecuteAsync(
        ProcessModel processModel,
        IContextBpmnProcess context,
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ConcurrentDictionary<string, string> errorRegistry,
        CancellationToken cancellationToken);
}