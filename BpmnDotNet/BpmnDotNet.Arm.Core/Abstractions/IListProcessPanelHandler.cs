using BpmnDotNet.Arm.Core.Dto;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IListProcessPanelHandler
{
    Task<int> GetCountAllPages(string idActiveProcess);
    Task<ListProcessPanelDto[]> GetPagesStates(string idActiveProcess, string lastToken, int countLineOnePage );
   
}