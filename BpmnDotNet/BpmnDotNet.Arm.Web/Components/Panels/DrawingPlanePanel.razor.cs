using BpmnDotNet.Arm.Core.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class DrawingPlanePanel : ComponentBase
{
    [Inject] private IPlanePanelHandler PlaneHandler { get; set; } = null!;
    
    [Inject] private  ILogger<DrawingPlanePanel> Logger { get; set; } = null!;
    
    [Inject] private  ISvgConstructor SvgConstructor { get; set; } = null!;
    
    
    private string WidthConvas => $"{800}px";
    private string HeightConvas => $"{400}px";
    
    private string _svgToString = String.Empty;
    
    private string CssScale()
    {
        var scaleCurrent = 1.0F;;
        var offsetX = 0;
        var offsetY = 0;
        return $"transform: scale({scaleCurrent}) translate({offsetX}px, {offsetY}px)";
    }
    
    private RenderFragment GetRenderAnnotation() =>
        async void (builder) =>
        {
            try
            {
       
                builder.AddMarkupContent(0, _svgToString);
            }
            catch (Exception e)
            {
                Logger.LogError("[DrawingPlanePanel] {@Exception}", e.Message);
            }
        };

    public async Task UpdatePanel()
    {
        var bpmnPlane =  await PlaneHandler.GetPlane();
        _svgToString = await SvgConstructor.CreatePlane(bpmnPlane);
      
        await InvokeAsync(() =>
        {
            StateHasChanged();
            return Task.CompletedTask;
        });
        
    }
}