using BpmnDotNet.Arm.Core.Dto;

namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IListProcessPanelHandler
{
    Task<ListProcessPanelDto[]> GetStates(string idActiveProcess);
}