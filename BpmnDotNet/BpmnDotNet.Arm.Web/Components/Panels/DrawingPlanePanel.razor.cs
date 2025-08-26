using BpmnDotNet.Arm.Core.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class DrawingPlanePanel : ComponentBase
{
    private string _svgToString = string.Empty;
    [Inject] private IPlanePanelHandler PlaneHandler { get; set; } = null!;

    [Inject] private ILogger<DrawingPlanePanel> Logger { get; set; } = null!;

    [Inject] private ISvgConstructor SvgConstructor { get; set; } = null!;


    private string WidthConvas => $"{100}%";
    private string HeightConvas => $"{100}%";

    private string CssScale()
    {
        var scaleCurrent = 1.0F;
        ;
        var offsetX = 0;
        var offsetY = 0;
        return $"transform: scale({scaleCurrent}) translate({offsetX}px, {offsetY}px)";
    }

    private RenderFragment GetRenderAnnotation()
    {
        return async void (builder) =>
        {
            try
            {
                var bpmnPlane = await PlaneHandler.GetPlane();
                _svgToString = await SvgConstructor.CreatePlane(bpmnPlane);

                builder.AddMarkupContent(0, _svgToString);
            }
            catch (Exception e)
            {
                Logger.LogError("[DrawingPlanePanel] {@Exception}", e.Message);
            }
        };
    }

    public async Task UpdatePanel()
    {
        var bpmnPlane = await PlaneHandler.GetPlane();
        _svgToString = await SvgConstructor.CreatePlane(bpmnPlane);

        await InvokeAsync(() =>
        {
            StateHasChanged();
            return Task.CompletedTask;
        });
    }
}