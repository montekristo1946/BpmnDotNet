using BlazorBrowserInteractLabeler.Web.Common;
using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class DrawingPlanePanel : ComponentBase
{
    private string _svgToString = string.Empty;
    [Inject] private IPlanePanelHandler PlaneHandler { get; set; } = null!;

    [Inject] private ILogger<DrawingPlanePanel> Logger { get; set; } = null!;

    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private string WidthConvas => $"{100}%";
    private string HeightConvas => $"{100}%";

    private string _idActiveProcess = string.Empty;

    private SizeWindows SizeWindows { get; set; } = new SizeWindows();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("window.registerViewportChangeCallback",
                DotNetObjectReference.Create(this));
            SizeWindows = await JSRuntime.InvokeAsync<SizeWindows>("GetBrowseSize", ConstantsArm.DrawingPlanePanel);
        }
    }

    [JSInvokable]
    public void OnResize(int width, int height)
    {
        InvokeAsync(async () =>
        {
            SizeWindows = await JSRuntime.InvokeAsync<SizeWindows>("GetBrowseSize", ConstantsArm.DrawingPlanePanel);
            await UpdatePanel();
        });
    }

    private string CssScale()
    {
        var scaleCurrent = 1.0F;
        var offsetX = 0;
        var offsetY = 0;
        return $"transform: scale({scaleCurrent}) translate({offsetX}px, {offsetY}px)";
    }

    private RenderFragment GetRenderAnnotation()
    {
        return void (builder) =>
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
    }

    public async Task UpdatePanel(string idProcess = "")
    {
        try
        {
            if (string.IsNullOrEmpty(idProcess))
            {
                return;
            }

            _idActiveProcess = idProcess;

            _svgToString = await PlaneHandler.GetPlane(_idActiveProcess, SizeWindows);
            await InvokeAsync(() =>
            {
                StateHasChanged();
                return Task.CompletedTask;
            });
        }
        catch (Exception e)
        {
            Logger.LogError("[UpdatePanel] {@Exception}", e.Message);
        }
    }
}