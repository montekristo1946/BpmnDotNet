using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.Dto;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class ListProcessPanel : ComponentBase
{
    [Parameter] public Func<string, Task> IsColorUpdateNodeJobStatus { get; set; } = null!;

    [Parameter] public Func<string, Task> IsBaseUpdateNodeJobStatus { get; set; } = null!;
    [Inject] private ILogger<ListProcessPanel> Logger { get; set; } = null!;

    [Inject] private IListProcessPanelHandler ListProcessPanelHandler { get; set; } = null!;

    private ListProcessPanelDto[] _listProcessPanel = [];
    private string _activeIdBpmnProcess = string.Empty;
    private ListProcessPanelDto _acitveTable = new();
    private int _currentPage = 0;

    private int _countLineOnePage = 10;
    private int _countAllPage = 0;
    private string[] _arrErrors = [];

    private string[] _filtersProcessStatus =
    [
        nameof(ProcessStatus.None), nameof(ProcessStatus.Works), nameof(ProcessStatus.Completed),
        nameof(ProcessStatus.Error)
    ];

    private string _filterToken = string.Empty;

    public async Task UpdatePanel()
    {
        try
        {
            await InvokeAsync(async () =>
            {
                var allprocessLine = await ListProcessPanelHandler
                    .GetCountAllRows(_activeIdBpmnProcess, _filtersProcessStatus);
                if (allprocessLine == 0)
                {
                    _listProcessPanel = [];
                    _countAllPage = 0;
                    _currentPage = 0;
                    ClearValue();
                }

                var from = _currentPage * _countLineOnePage;
                var currentList = await ListProcessPanelHandler
                    .GetPagesStates(_activeIdBpmnProcess, from, _countLineOnePage, _filtersProcessStatus);

                if (currentList.Any() is false)
                {
                    return;
                }

                _listProcessPanel = currentList;
                _countAllPage = (int)Math.Ceiling((double)allprocessLine / _countLineOnePage);
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
        _arrErrors = [];
        _acitveTable = new ListProcessPanelDto();
    }

    public async Task SetIdProcess(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _activeIdBpmnProcess = string.Empty;
            return;
        }

        _activeIdBpmnProcess = value;
        ClearValue();
        await UpdatePanel();
    }


    private async Task ButtonClickObjectAsync(ListProcessPanelDto table)
    {
        _acitveTable = table;
        await IsColorUpdateNodeJobStatus(table.IdStorageHistoryNodeState);

        _arrErrors = await ListProcessPanelHandler.GetErrors(table.IdStorageHistoryNodeState);
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


    private async Task Forward()
    {
        if (_currentPage >= _countAllPage - 1)
        {
            return;
        }

        ClearValue();
        _currentPage += 1;
        await IsBaseUpdateNodeJobStatus(_activeIdBpmnProcess);

        await UpdatePanel();
    }

    private async Task Backward()
    {
        if (_currentPage <= 0)
        {
            return;
        }

        _currentPage -= 1;
        await IsBaseUpdateNodeJobStatus(_activeIdBpmnProcess);
        ClearValue();
        await UpdatePanel();
    }

    public async Task SetStatusFilter(string[] filters)
    {
        _filtersProcessStatus = filters;
        ClearValue();
        await UpdatePanel();
    }

    public async Task SetFilterToken(string filterToken)
    {
        _filterToken = filterToken;
        await UpdateFilterToken();
    }

    public async Task UpdateFilterToken()
    {
        try
        {
            _listProcessPanel = [];
            _countAllPage = 0;
            _currentPage = 0;
            ClearValue();


            var sizeSample = 100;
            var currentList = await ListProcessPanelHandler
                .GetHistoryNodeFromTokenMaskAsync(_activeIdBpmnProcess, _filterToken, sizeSample);

            if (currentList.Any() is false)
            {
                return;
            }

            _listProcessPanel = currentList;
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
}