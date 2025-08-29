using BpmnDotNet.Arm.Web.Components.Panels;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Pages;

public partial class Operate : ComponentBase
{
    private RenderFragment BpmnPlanePanelTemplate { get; set; }
    private DrawingPlanePanel? _drawingPlanePanel;
    private RenderFragment MenuPanelTemplate { get; set; }
    private MenuPanel? _menuPanel;
    private RenderFragment FilterPanelTemplate { get; set; }
    private FilterPanel? _filterPanel;
    
    protected override Task OnInitializedAsync()
    {
        BpmnPlanePanelTemplate = CreateBpmnPlanePanelTemplate();
        MenuPanelTemplate = CreateMenuPanelTemplate();
        FilterPanelTemplate = CreateFilterPanelTemplate();
        return Task.CompletedTask;
    }

    private RenderFragment CreateFilterPanelTemplate()
    {
        return builder =>
        {
            builder.OpenComponent(0, typeof(FilterPanel));
            builder.AddAttribute(1, nameof(FilterPanel.HandlerSetupIdProcess), HandlerSetupIdProcess);
            builder.AddComponentReferenceCapture(1, value =>
            {
                _filterPanel = value as FilterPanel
                               ?? throw new InvalidOperationException(
                                   "Не смог c конвертировать FilterPanel в FilterPanel");
            });

            builder.CloseComponent();
        };
    }

    private void HandlerSetupIdProcess( string  value )
    {
        _drawingPlanePanel?.UpdatePanel(value);
    }

    private RenderFragment CreateBpmnPlanePanelTemplate()
    {
        return builder =>
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
    }

    private RenderFragment CreateMenuPanelTemplate()
    {
        return builder =>
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
    }

    private void UpdateUpdatePanel()
    {

        _drawingPlanePanel?.UpdatePanel();

    }
}