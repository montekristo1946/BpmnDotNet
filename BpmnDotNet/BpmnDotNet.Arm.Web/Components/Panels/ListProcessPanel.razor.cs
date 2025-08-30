using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class ListProcessPanel : ComponentBase
{
    [Parameter] public Action<string> ChoseTokenProcess { get; set; } = null!;
    
    [Inject] private ILogger<ListProcessPanel> Logger { get; set; } = null!;
    
    [Inject] private IListProcessPanelHandler ListProcessPanelHandler { get; set; } = null!;
    
    private ListProcessPanelDto [] _listProcessPanel =[];
    private string _idActiveProcess = string.Empty;
    private ListProcessPanelDto _acitveTable =  new ListProcessPanelDto();
    
    public void UpdatePanel()
    {
        try
        {
            InvokeAsync( () =>
            {
                _listProcessPanel =  ListProcessPanelHandler.GetStates(_idActiveProcess).Result;
                StateHasChanged();
                return Task.CompletedTask;
            });
        }
        catch (Exception e)
        {
            Logger.LogError("[ListProcessPanel:UpdatePanel] {@Exception}", e.Message);
        }
    }

    public void SetIdProcess(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _idActiveProcess = string.Empty;
            return;
        }
        
        _idActiveProcess =  value;
    }

    private string GetColorState(ProcessState tableState)
    {
        return tableState switch
        {
            ProcessState.Running => "#19aee8",
            ProcessState.None => "#212529",
            ProcessState.Completed => "#00ae5e",
            ProcessState.Error => "#f34848",
            _ => "#212529"
        };
    }

    private Task ButtonClickObjectAsync(ListProcessPanelDto table)
    {
        _acitveTable = table;
        return Task.CompletedTask;
    }
}