using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Dto;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IListProcessPanelHandler
{
    Task<int> GetCountAllPages(string idActiveProcess, string [] processStatus = null);

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
    ///     Вернет список ошибок по данному процессую
    /// </summary>
    /// <param name="idUpdateNodeJobStatus"></param>
    /// <returns></returns>
    Task<string[]> GetErrors(string idUpdateNodeJobStatus);
    
}