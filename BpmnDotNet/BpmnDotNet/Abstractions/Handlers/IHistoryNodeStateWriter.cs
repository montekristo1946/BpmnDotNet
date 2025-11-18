namespace BpmnDotNet.Abstractions.Handlers;

using BpmnDotNet.Common.Dto;

/// <summary>
/// Запишет NodeJobStatus в хранилище отчетов.
/// </summary>
internal interface IHistoryNodeStateWriter
{
    /// <summary>
    /// Записать данные.
    /// </summary>
    /// <param name="idBpmnProcess">ID процесса.</param>
    /// <param name="tokenProcess">Токен.</param>
    /// <param name="nodeStateRegistry">Состояние нод.</param>
    /// <param name="arrayMessageErrors">Массив с текстом ошибок.</param>
    /// <param name="isCompleted">Флаг. Финальная запись проходит.</param>
    /// <param name="dateFromInitInstance">Временная метка создания отчета.</param>
    /// <returns> <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetStateProcessAsync(
        string idBpmnProcess,
        string tokenProcess,
        NodeJobStatus[] nodeStateRegistry,
        string[] arrayMessageErrors,
        bool isCompleted,
        long dateFromInitInstance);
}