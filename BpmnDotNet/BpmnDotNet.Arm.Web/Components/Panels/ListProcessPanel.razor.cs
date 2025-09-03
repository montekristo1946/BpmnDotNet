using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Dto;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class ListProcessPanel : ComponentBase
{
    [Parameter] public Action<string> IsColorUpdateNodeJobStatus { get; set; } = null!;
    
    [Parameter] public Action<string> IsBaseUpdateNodeJobStatus { get; set; } = null!;

    [Inject] private ILogger<ListProcessPanel> Logger { get; set; } = null!;

    [Inject] private IListProcessPanelHandler ListProcessPanelHandler { get; set; } = null!;

    private ListProcessPanelDto[] _listProcessPanel = [];
    private string _activeIdBpmnProcess = string.Empty;
    private ListProcessPanelDto _acitveTable = new ListProcessPanelDto();
    private int _currentPage = 0;

    private int _countLineOnePage = 10;
    private int _countAllPage = 0;
    // private string _curentToken = string.Empty;
    // private Stack<string> _lastTokens = new();
    private string[] _arrErrors = [];


    public void UpdatePanel()
    {
        try
        {
            InvokeAsync(() =>
            {
                var allprocessLine = ListProcessPanelHandler.GetCountAllPages(_activeIdBpmnProcess, null).Result;
                if (allprocessLine == 0)
                {
                    _listProcessPanel = [];
                    _countAllPage = 0;
                    _currentPage = 0;
                    ClearValue();
                }

                var from = _currentPage * _countLineOnePage;
                var currentList = ListProcessPanelHandler
                    .GetPagesStates(_activeIdBpmnProcess, from, _countLineOnePage, null).Result;
                
                if (currentList.Any() is false)
                {
                    return Task.CompletedTask;
                }

                _listProcessPanel = currentList;
                _countAllPage = (int)Math.Ceiling((double)allprocessLine / _countLineOnePage);

                return Task.CompletedTask;
            });
        }
        catch (Exception e)
        {
            Logger.LogError("[ListProcessPanel:UpdatePanel] {@Exception}", e.Message);
        }
        finally
        {
            StateHasChanged();
        }
    }

    private void ClearValue()
    {
        _arrErrors =  [];
        _acitveTable = new ListProcessPanelDto();
    }
    public void SetIdProcess(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _activeIdBpmnProcess = string.Empty;
            return;
        }

        _activeIdBpmnProcess = value;
        ClearValue();
        UpdatePanel();
    }

    
    private async Task ButtonClickObjectAsync(ListProcessPanelDto table)
    {
        _acitveTable = table;
        IsColorUpdateNodeJobStatus?.Invoke(table.IdStorageHistoryNodeState);
        
        _arrErrors  = await ListProcessPanelHandler.GetErrors(table.IdStorageHistoryNodeState);
    }



    private string GetColorState(ProcessState tableState)
    {
        return tableState switch
        {
            ProcessState.Works => "#19aee8",
            ProcessState.None => "#212529",
            ProcessState.Completed => "#00ae5e",
            ProcessState.Error => "#f34848",
            _ => "#212529"
        };
    }


    private void Forward()
    {
        if (_currentPage >= _countAllPage)
        {
            return;
        }

        ClearValue();
        // _curentToken = _listProcessPanel.LastOrDefault()?.TokenProcess ?? string.Empty;
        _currentPage += 1;
        IsBaseUpdateNodeJobStatus?.Invoke(_activeIdBpmnProcess);
       
        UpdatePanel();
    }

    private void Backward()
    {
        if (_currentPage <= 0)
        {
            return;
        }


        // _curentToken = token ?? string.Empty;

        _currentPage -= 1;
        IsBaseUpdateNodeJobStatus?.Invoke(_activeIdBpmnProcess);
        ClearValue();
        UpdatePanel();
    }
}