using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Dto;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IListProcessPanelHandler
{
    Task<int> GetCountAllPages(string idActiveProcess);
    Task<ListProcessPanelDto[]> GetPagesStates(string idActiveProcess, string lastToken, int countLineOnePage );
    
    /// <summary>
    ///     Вернет список ошибок по данному процессую
    /// </summary>
    /// <param name="idUpdateNodeJobStatus"></param>
    /// <returns></returns>
    Task<string[]> GetErrors(string idUpdateNodeJobStatus);
    
}