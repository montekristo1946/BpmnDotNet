using BpmnDotNet.Arm.Web.Components.Panels;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Pages;

public partial class Operate : ComponentBase
{
    private RenderFragment BpmnPlanePanelTemplate { get; set; }
    private DrawingPlanePanel? _drawingPlanePanel = null;
    private RenderFragment MenuPanelTemplate{ get; set; }
    private MenuPanel? _menuPanel = null;
    protected override async Task OnInitializedAsync()
    {
        BpmnPlanePanelTemplate = CreateBpmnPlanePanelTemplate();
        MenuPanelTemplate = CreateMenuPanelTemplate();
    }

    private RenderFragment CreateBpmnPlanePanelTemplate() => builder =>
    {
        builder.OpenComponent(0, typeof(DrawingPlanePanel));
        builder.AddComponentReferenceCapture(1, value =>
        {
            _drawingPlanePanel = value as DrawingPlanePanel
                                 ?? throw new InvalidOperationException(
                                     "Не смог c конвертировать DrawingPlanePanel в DrawingPlanePanel");
        });

        builder.CloseComponent();
    };
    
    private RenderFragment CreateMenuPanelTemplate() => builder =>
    {
        builder.OpenComponent(0, typeof(MenuPanel));
        builder.AddAttribute(1, nameof(MenuPanel.IsUpdatePanel), UpdateUpdatePanel);
        builder.AddComponentReferenceCapture(2, value =>
        {
            _menuPanel = value as MenuPanel
                         ?? throw new InvalidOperationException(
                             "Не смог c конвертировать MenuPanel в MenuPanel");
        });

        builder.CloseComponent();
    };
    
    private void UpdateUpdatePanel()
    {
        _drawingPlanePanel?.UpdatePanel().Wait();
    }

}