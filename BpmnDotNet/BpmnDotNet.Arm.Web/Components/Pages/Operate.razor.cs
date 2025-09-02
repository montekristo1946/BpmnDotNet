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
    public RenderFragment ListProcessPanelTemplate { get; set; }
    private ListProcessPanel? _listProcessPanel;
    
    
    protected override Task OnInitializedAsync()
    {
        BpmnPlanePanelTemplate = CreateBpmnPlanePanelTemplate();
        MenuPanelTemplate = CreateMenuPanelTemplate();
        FilterPanelTemplate = CreateFilterPanelTemplate();
        ListProcessPanelTemplate = CreateListProcessPanelTemplate();
        return Task.CompletedTask;
    }
    
    private void ChoseIdProcess( string  value )
    {
        _drawingPlanePanel?.BaseUpdatePanel(value);
        _listProcessPanel?.SetIdProcess(value);
    }
    private void IsColorUpdateNodeJobStatus( string  idUpdateNodeJobStatus )
    {
        _drawingPlanePanel?.ColorUpdatePanel(idUpdateNodeJobStatus);
    }
    
    private void UpdateUpdatePanel()
    {
        _filterPanel?.UpdatePanel();
        _drawingPlanePanel?.BaseUpdatePanel();
        _listProcessPanel?.UpdatePanel();
        
    }

    private RenderFragment CreateListProcessPanelTemplate()
    {
        return builder =>
        {
            builder.OpenComponent(0, typeof(ListProcessPanel));
            builder.AddAttribute(1, nameof(ListProcessPanel.IsColorUpdateNodeJobStatus), IsColorUpdateNodeJobStatus);
            builder.AddAttribute(2, nameof(ListProcessPanel.IsBaseUpdateNodeJobStatus), ChoseIdProcess);
            builder.AddComponentReferenceCapture(3, value =>
            {
                _listProcessPanel = value as ListProcessPanel
                                    ?? throw new InvalidOperationException(
                                        "Не смог c конвертировать ListProcessPanel в ListProcessPanel");
            });

            builder.CloseComponent();
        };
    }

    

    private RenderFragment CreateFilterPanelTemplate()
    {
        return builder =>
        {
            builder.OpenComponent(0, typeof(FilterPanel));
            builder.AddAttribute(1, nameof(FilterPanel.ChoseIdProcess), ChoseIdProcess);
            builder.AddComponentReferenceCapture(1, value =>
            {
                _filterPanel = value as FilterPanel
                               ?? throw new InvalidOperationException(
                                   "Не смог c конвертировать FilterPanel в FilterPanel");
            });

            builder.CloseComponent();
        };
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

  
}