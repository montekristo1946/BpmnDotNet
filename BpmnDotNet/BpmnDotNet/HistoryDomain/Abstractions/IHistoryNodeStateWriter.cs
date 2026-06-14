using System.Collections.Concurrent;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.Dto;

namespace BpmnDotNet.HistoryDomain.Abstractions;

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
    /// <param name="nodeStateRegistry">Регистр состояние нод.</param>
    /// <param name="errorRegistry">Регистр ошибок.</param>
    /// <param name="dateFromInitInstance">Временная метка создания отчета.</param>
    /// <returns> <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetStateProcessAsync(
        string idBpmnProcess,
        string tokenProcess,
        ConcurrentDictionary<string, StatusNode> nodeStateRegistry,
        ConcurrentDictionary<string, string> errorRegistry,
        long dateFromInitInstance);
}