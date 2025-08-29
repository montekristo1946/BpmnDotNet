using BpmnDotNet.Arm.Core.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class FilterPanel : ComponentBase
{
    [Inject] private IFilterPanelHandler FilterPanelHandler { get; set; } = null!;
    
    [Parameter] public Action<string> HandlerSetupIdProcess { get; set; } = null!;

    private string[] _arrayProcessId = [];

    protected override async Task OnInitializedAsync()
    {
        _arrayProcessId = await FilterPanelHandler.GetAllProcessId();
    }

    private Task ButtonClickObjectAsync(string process)
    {
        HandlerSetupIdProcess?.Invoke(process);
        return Task.CompletedTask;
    }
}