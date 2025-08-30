using BpmnDotNet.Arm.Core.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class FilterPanel : ComponentBase
{
    [Inject] private IFilterPanelHandler FilterPanelHandler { get; set; } = null!;
    
    [Inject] private ILogger<FilterPanel> Logger { get; set; } = null!;
    
    [Parameter] public Action<string> ChoseIdProcess { get; set; } = null!;

    private string[] _arrayProcessId = [];

    private Task ButtonClickObjectAsync(string process)
    {
        ChoseIdProcess?.Invoke(process);
        return Task.CompletedTask;
    }

    public void UpdatePanel()
    {
        try
        {
             InvokeAsync(() =>
            {
                _arrayProcessId =  FilterPanelHandler.GetAllProcessId().Result;
                StateHasChanged();
                return Task.CompletedTask;
            });
        }
        catch (Exception e)
        {
            Logger.LogError("[FilterPanel:UpdatePanel] {@Exception}", e.Message);
        }
    }
}