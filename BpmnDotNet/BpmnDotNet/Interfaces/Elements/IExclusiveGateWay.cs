using System.Collections.Concurrent;

namespace BpmnDotNet.Interfaces.Elements;

/// <summary>
/// Маршруты для эксклюзивных шлюзов.
/// </summary>
public interface IExclusiveGateWay
{
    /// <summary>
    /// Маршруты для эксклюзивных шлюзов, где: (IdGateWay, idFlowRoute).
    /// </summary>
    public ConcurrentDictionary<string, string> ConditionRoute { get; init; }
}