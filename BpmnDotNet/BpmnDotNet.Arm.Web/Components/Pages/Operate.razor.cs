using BpmnDotNet.Arm.Web.Components.Panels;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Pages;

public partial class Operate : ComponentBase
{
    private RenderFragment BpmnPlanePanelTemplate { get; set; }
    private DrawingPlanePanel? _drawingPlanePanel = null;
    
    
    protected override async Task OnInitializedAsync()
    {
        BpmnPlanePanelTemplate = CreateBpmnPlanePanelTemplate();
    }

    private RenderFragment CreateBpmnPlanePanelTemplate() => builder =>
    {
        builder.OpenComponent(0, typeof(DrawingPlanePanel));


        builder.AddComponentReferenceCapture(4, value =>
        {
            _drawingPlanePanel = value as DrawingPlanePanel
                                 ?? throw new InvalidOperationException(
                                     "Не смог c конвертировать DrawingPlanePanel в DrawingPlanePanel");
        });

        builder.CloseComponent();
    };

}