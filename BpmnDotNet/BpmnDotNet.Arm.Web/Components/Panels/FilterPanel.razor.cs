using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Common.Dto;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class FilterPanel : ComponentBase
{
    [Inject] private IFilterPanelHandler FilterPanelHandler { get; set; } = null!;

    [Inject] private ILogger<FilterPanel> Logger { get; set; } = null!;

    [Parameter] public Action<string> ChoseIdProcess { get; set; } = null!;

    [Parameter] public Action<string[]> SetStatusFilter { get; set; } = null!;

    [Parameter] public Action<string> SetFilterToken { get; set; } = null!;

    private string[] _arrayProcessId = [];
    private bool _isCheckFilterNone { get; set; } = true;
    private bool _isCheckFilterWorks { get; set; } = true;
    private bool _isCheckFilterCompleted { get; set; } = true;
    private bool _isCheckFilterError { get; set; } = true;
    private string _filterToken = " *part token*";

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
                _arrayProcessId = FilterPanelHandler.GetAllProcessId().Result;
                StateHasChanged();
                return Task.CompletedTask;
            });
        }
        catch (Exception e)
        {
            Logger.LogError("[FilterPanel:UpdatePanel] {@Exception}", e.Message);
        }
    }

    private string[] CreateFilterStatus()
    {
        var retArr = new List<string>();
        if (_isCheckFilterNone)
        {
            retArr.Add(nameof(ProcessStatus.None));
        }

        if (_isCheckFilterError)
        {
            retArr.Add(nameof(ProcessStatus.Error));
        }

        if (_isCheckFilterCompleted)
        {
            retArr.Add(nameof(ProcessStatus.Completed));
        }

        if (_isCheckFilterWorks)
        {
            retArr.Add(nameof(ProcessStatus.Works));
        }

        return [.. retArr];
    }

    private void ChangeNone(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            _isCheckFilterNone = newValue;
        }
        var processStatus = CreateFilterStatus();
        SetStatusFilter?.Invoke(processStatus);
    }

    private void ChangeWorks(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            _isCheckFilterWorks = newValue;
        }

        var processStatus = CreateFilterStatus();
        SetStatusFilter?.Invoke(processStatus);
    }

    private void ChangeCompleted(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            _isCheckFilterCompleted = newValue;
        }
        var processStatus = CreateFilterStatus();
        SetStatusFilter?.Invoke(processStatus);
    }

    private void ChangeError(ChangeEventArgs obj)
    {
        if (obj?.Value is bool newValue)
        {
            _isCheckFilterError = newValue;
        }
        var processStatus = CreateFilterStatus();
        SetStatusFilter?.Invoke(processStatus);
    }

    private void OnChangeFilterToken(ChangeEventArgs changeEventArgs)
    {
        if (changeEventArgs?.Value == null )
            return;

        _filterToken = changeEventArgs.Value.ToString() ?? string.Empty;
        
    }

    private void OnclickButtonSearch()
    {
        SetFilterToken?.Invoke(_filterToken);
    }
}