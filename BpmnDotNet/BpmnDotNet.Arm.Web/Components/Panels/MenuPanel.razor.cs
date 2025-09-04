using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class MenuPanel : ComponentBase
{
    [Parameter] public Action? IsUpdatePanel { get; set; }

    private void OnClickUpdate()
    {
        IsUpdatePanel?.Invoke();

    }
}