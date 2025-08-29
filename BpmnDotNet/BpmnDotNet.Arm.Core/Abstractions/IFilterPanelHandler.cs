namespace BpmnDotNet.Arm.Core.Abstractions;

public interface IFilterPanelHandler
{
   public Task<string[]> GetAllProcessId();
}