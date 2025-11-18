using System.Runtime.CompilerServices;
using BpmnDotNet.Arm.Core.Dto;
using BpmnDotNet.Common.BPMNDiagram;

[assembly: InternalsVisibleTo("BpmnDotNet.Arm.Web")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Arm.Core.Abstractions;

/// <summary>
///     Handler для работы PlanePane.
/// </summary>
internal interface IPlanePanelHandler
{
    /// <summary>
    ///     Вернет BpmnPlane для отрисовки.
    /// </summary>
    /// <param name="IdBpmnProcess"></param>
    /// <param name="sizeWindows"></param>
    public Task<string> GetPlane(string IdBpmnProcess, SizeWindows sizeWindows);

    /// <summary>
    ///     Вернет BpmnPlane с раскрашенными состояниями.
    /// </summary>
    /// <param name="idUpdateNodeJobStatus"></param>
    /// <param name="sizeWindows"></param>
    /// <returns></returns>
    public Task<string> GetColorPlane(string idUpdateNodeJobStatus, SizeWindows sizeWindows);

    /// <summary>
    /// Установить масштаб.
    /// </summary>
    /// <param name="deltaY">The vertical scroll amount.</param>
    /// <param name="scaleCurrent"></param>
    /// <returns></returns>
    public double SetScrollValue(double? deltaY, double scaleCurrent);

    /// <summary>
    /// Рассчитает точку смещения svg.
    /// </summary>
    /// <param name="pointStart">Точка начало движения</param>
    /// <param name="pointEnd">Точка окончания движения.</param>
    /// <param name="currentOffset">Текущее смещение.</param>
    /// <param name="scaleCurrent">Текущий масштаб</param>
    /// <returns>Скорректированное смещение.</returns>
    public PointT CalculateOffset(PointT pointStart, PointT pointEnd, PointT currentOffset, double scaleCurrent);
}