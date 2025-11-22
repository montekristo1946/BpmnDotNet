namespace BpmnDotNet.Arm.Core.Abstractions;

using BpmnDotNet.Arm.Core.Dto;

/// <summary>
/// Контракт service ListProcessPanel.
/// </summary>
internal interface IListProcessPanelHandler
{
    /// <summary>
    /// Общее количество отсортированных строк по processStatus.
    /// </summary>
    /// <param name="idActiveProcess">ID активного процесса.</param>
    /// <param name="processStatus"> Статусы процессов.</param>
    /// <returns>Количество найденных строк.</returns>
    Task<int> GetCountAllRows(string idActiveProcess, string[] processStatus);

    /// <summary>
    /// Страницы для отрисовки.
    /// </summary>
    /// <param name="idBpmnProcess">ID BPMN процесса.</param>
    /// <param name="skip">from.</param>
    /// <param name="take">take.</param>
    /// <param name="processStatus">Список процесс статусов.</param>
    /// <returns>ДТО processPanel.</returns>
    Task<ListProcessPanelDto[]> GetPagesStates(
        string idBpmnProcess,
        int skip,
        int take,
        string[] processStatus);

    /// <summary>
    ///     Вернет список ошибок по данному процессу.
    /// </summary>
    /// <param name="idUpdateNodeJobStatus">IdBpmnProcess+TokenProcess.</param>
    /// <returns>Массив текста ошибок.</returns>
    Task<string[]> GetErrors(string idUpdateNodeJobStatus);

    /// <summary>
    ///  Страницы для отрисовки. Отсортированные по filterToken (маске).
    /// </summary>
    /// <param name="idBpmnProcess">ID BPMN процесса.</param>
    /// <param name="filterToken">>Маска поиска формата elastic.</param>
    /// <param name="sizeSample">Количество выдаваемого результата.</param>
    /// <returns>ДТО processPanel.</returns>
    Task<ListProcessPanelDto[]> GetHistoryNodeFromTokenMaskAsync(string idBpmnProcess, string filterToken, int sizeSample);
}