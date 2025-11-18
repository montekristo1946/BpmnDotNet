namespace BpmnDotNet.Abstractions.Handlers;

/// <summary>
/// Работает с Description.
/// </summary>
internal interface IDescriptionWriteService
{
    /// <summary>
    /// Добавить описания на блок выполнения.
    /// </summary>
    /// <param name="taskDefinitionId"> Id блочка в Bpmn нотации.</param>
    /// <param name="description">Описание процесса.</param>
    void AddDescription(string taskDefinitionId, string description);

    /// <summary>
    /// Записать дескрипторы в базу данных.
    /// </summary>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Task.</returns>
    Task CommitAsync(CancellationToken token = default);

    /// <summary>
    /// Инициализировать новый инстанс.
    /// </summary>
    /// <param name="token">Токен отмены.</param>
    /// <returns>Task.</returns>
    Task InitAsync(CancellationToken token = default);
}