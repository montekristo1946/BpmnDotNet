using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        NavManager.NavigateTo("Operate");
        return Task.CompletedTask;
    }
}