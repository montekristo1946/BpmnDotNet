using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Dto;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class FilterPanel : ComponentBase
{
    [Inject] private IFilterPanelHandler FilterPanelHandler { get; set; } = null!;

    [Inject] private ILogger<FilterPanel> Logger { get; set; } = null!;

    [Parameter] public Func<string, Task> ChoseIdProcess { get; set; } = null!;

    [Parameter] public Func<string[], Task> SetStatusFilter { get; set; } = null!;

    [Parameter] public Func<string, Task> SetFilterToken { get; set; } = null!;

    private ProcessDataFilterPanel[] _arrayProcessData = [];
    private bool IsCheckFilterNone { get; set; } = true;
    private bool IsCheckFilterWorks { get; set; } = true;
    private bool IsCheckFilterCompleted { get; set; } = true;
    private bool IsCheckFilterError { get; set; } = true;
    
    private string _filterToken = "*value*";
    
    private string _activeIdProcessData = String.Empty;

    private async Task ButtonClickObjectAsync(string process)
    {
        _activeIdProcessData = process ;
        await ChoseIdProcess(process);
    }

    public async Task UpdatePanel()
    {
        try
        {
            _arrayProcessData = await FilterPanelHandler.GetAllProcessIdAsync();
            StateHasChanged();
        }
        catch (Exception e)
        {
            Logger.LogError("[FilterPanel:UpdatePanel] {@Exception}", e.Message);
        }
    }

    private string[] CreateFilterStatus()
    {
        var retArr = new List<string>();
        if (IsCheckFilterNone)
        {
            retArr.Add(nameof(ProcessStatus.None));
        }

        if (IsCheckFilterError)
        {
            retArr.Add(nameof(ProcessStatus.Error));
        }

        if (IsCheckFilterCompleted)
        {
            retArr.Add(nameof(ProcessStatus.Completed));
        }

        if (IsCheckFilterWorks)
        {
            retArr.Add(nameof(ProcessStatus.Works));
        }

        return [.. retArr];
    }

    private async Task ChangeNone(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            IsCheckFilterNone = newValue;
        }

        var processStatus = CreateFilterStatus();
        await SetStatusFilter(processStatus);
    }

    private async Task ChangeWorks(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            IsCheckFilterWorks = newValue;
        }

        var processStatus = CreateFilterStatus();
        await SetStatusFilter(processStatus);
    }

    private async Task ChangeCompleted(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            IsCheckFilterCompleted = newValue;
        }

        var processStatus = CreateFilterStatus();
        await SetStatusFilter(processStatus);
    }

    private async Task ChangeError(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            IsCheckFilterError = newValue;
        }

        var processStatus = CreateFilterStatus();
        await SetStatusFilter(processStatus);
    }

    private void OnChangeFilterToken(ChangeEventArgs changeEventArgs)
    {
        if (changeEventArgs?.Value == null)
            return;

        _filterToken = changeEventArgs.Value.ToString() ?? string.Empty;
    }

    private async Task OnclickButtonSearch()
    {
        await SetFilterToken(_filterToken);
    }
}