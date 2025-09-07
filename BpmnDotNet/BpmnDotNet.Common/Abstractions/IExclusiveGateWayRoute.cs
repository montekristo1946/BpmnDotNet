namespace BpmnDotNet.Common.Abstractions;

using System.Collections.Concurrent;

/// <summary>
///     Маршруты для эксклюзивных шлюзов.
/// </summary>
public interface IExclusiveGateWayRoute
{
    /// <summary>
    ///     Gets маршруты для эксклюзивных шлюзов, где: (IdGateWay, idFlowRoute).
    /// </summary>
    public ConcurrentDictionary<string, string> ConditionRoute { get; init; }
}