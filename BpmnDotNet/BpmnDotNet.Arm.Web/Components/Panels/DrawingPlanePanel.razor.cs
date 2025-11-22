using BlazorBrowserInteractLabeler.Web.Common;
using BpmnDotNet.Arm.Core.Common;
using BpmnDotNet.Arm.Core.UiDomain.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BpmnDotNet.Arm.Web.Components.Panels;

public partial class DrawingPlanePanel : ComponentBase
{
    [Inject] private IPlanePanelHandler PlaneHandler { get; set; } = null!;

    [Inject] private ILogger<DrawingPlanePanel> Logger { get; set; } = null!;

    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private string WidthConvas => $"{100}%";
    private string HeightConvas => $"{100}%";

    private double ScaleCurrent = 1.0;

    private string _idActiveProcess = string.Empty;


    private SizeWindows SizeWindows { get; set; } = new ();
    private string _svgToString = string.Empty;
    private bool _isMoveSvg = false;
    private const int LeftButton = 1;
    private const int RightButton = 2;
    private PointT _pointStartOffset = new(){X = 0,Y = 0};
    private PointT _offset = new(){X = 0,Y = 0};
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
            await BaseUpdatePanel(_idActiveProcess);
        });
    }

    private string CssScale()
    {
        return $"transform: scale({ScaleCurrent}) translate({_offset.X}px, {_offset.Y}px)";
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

    public async Task BaseUpdatePanel(string idProcess = "")
    {
        try
        {
            if (string.IsNullOrEmpty(idProcess))
            {
                return;
            }
            ResetScale();
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

    public async Task ColorUpdatePanel(string idUpdateNodeJobStatus)
    {
        try
        {
            if (string.IsNullOrEmpty(idUpdateNodeJobStatus))
            {
                return;
            }

            ResetScale();
            _svgToString = await PlaneHandler.GetColorPlane(idUpdateNodeJobStatus, SizeWindows);
            StateHasChanged();
        }
        catch (Exception e)
        {
            Logger.LogError("[ColorUpdatePanel] {@Exception}", e.Message);
        }
    }

    private void MouseWheelHandler(WheelEventArgs args)
    {
        ScaleCurrent = PlaneHandler.SetScrollValue(args?.DeltaY, ScaleCurrent);
    }

    private void MouseMoveHandler(MouseEventArgs obj)
    {
        if (_isMoveSvg is false)
        {
            return;
        }

        var currentPoint = new PointT() { X = obj.PageX, Y = obj.PageY };
        _offset = PlaneHandler.CalculateOffset(_pointStartOffset, currentPoint,_offset,ScaleCurrent);
        _pointStartOffset = currentPoint;
        
    }

    private void OnmouseDownHandler(MouseEventArgs obj)
    {
        if (obj.Buttons == LeftButton)
        {
            _pointStartOffset = new PointT() { X = obj.PageX, Y = obj.PageY };
            _isMoveSvg = true;
        }

        if (obj.Buttons == RightButton)
        {
            ResetScale();
        }
    }

    private void OnmouseUpHandler(MouseEventArgs obj)
    {
        _isMoveSvg = false;
    }

    private void ResetScale()
    {
        _offset  = new PointT();
        ScaleCurrent = 1.0;
    }
}