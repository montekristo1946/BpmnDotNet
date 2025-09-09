using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class MenuPanel : ComponentBase
{
    [Parameter] public Func<Task> IsUpdatePanel { get; set; } = null!;

    private async Task OnClickUpdate()
    {
        await IsUpdatePanel();
    }
}