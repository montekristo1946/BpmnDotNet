using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Dto;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IListProcessPanelHandler
{
    /// <summary>
    /// Общее количество отсортированных по processStatus.
    /// </summary>
    /// <param name="idActiveProcess"></param>
    /// <param name="processStatus"></param>
    /// <returns></returns>
    Task<int> GetCountAllPages(string idActiveProcess, string[] processStatus = null);

    /// <summary>
    /// Страницы для отрисовки.
    /// </summary>
    /// <param name="idBpmnProcess"></param>
    /// <param name="from"></param>
    /// <param name="size"></param>
    /// <param name="processStatus"></param>
    /// <returns></returns>
    Task<ListProcessPanelDto[]> GetPagesStates(
        string idBpmnProcess,
        int from,
        int size,
        string[] processStatus = null);

    /// <summary>
    ///     Вернет список ошибок по данному процесу.
    /// </summary>
    /// <param name="idUpdateNodeJobStatus"></param>
    /// <returns></returns>
    Task<string[]> GetErrors(string idUpdateNodeJobStatus);

    /// <summary>
    ///  Страницы для отрисовки. Отсортированные по filterToken (маске).
    /// </summary>
    /// <param name="activeIdBpmnProcess"></param>
    /// <param name="filterToken"></param>
    /// <param name="sizeSample"></param>
    /// <returns></returns>
    Task<ListProcessPanelDto[]> GetHistoryNodeFromTokenMaskAsync(string activeIdBpmnProcess, string filterToken, int sizeSample);
}