namespace BpmnDotNet.Arm.Core.SvgDomain.Abstractions;

using BpmnDotNet.BPMNDiagram;

/// <summary>
/// Конструктор waypoint.
/// </summary>
/// <typeparam name="T">Тип блока.</typeparam>
internal interface IWayPointPosition<out T>
    where T : new()
{
    /// <summary>
    /// Добавить Титл.
    /// </summary>
    /// <param name="waypoints">Точки путей bpmn.</param>
    /// <returns>Вернет текущий блок.</returns>
    public T AddWayPoint(Waypoint[] waypoints);
}
