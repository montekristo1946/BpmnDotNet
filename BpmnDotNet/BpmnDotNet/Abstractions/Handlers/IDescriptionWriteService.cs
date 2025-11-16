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
    /// <returns>Task.</returns>
    Task Commit();

    /// <summary>
    /// Инициализировать новый инстанс.
    /// </summary>
    /// <returns>Task.</returns>
    Task Init();
}