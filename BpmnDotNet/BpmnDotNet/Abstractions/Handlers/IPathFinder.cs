namespace BpmnDotNet.Abstractions.Handlers;

using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Common.Abstractions;

/// <summary>
/// Инструмент поиска путей дальнейшего выполнения процесса.
/// </summary>
internal interface IPathFinder
{
    /// <summary>
    /// Найдет компонент StartEvent.
    /// </summary>
    /// <param name="elementsSrc">Список компонентов.</param>
    /// <returns>Вернет компоненты соответсвующие критерию поиска.</returns>
    IElement[] GetStartEvent(IElement[]? elementsSrc);

    /// <summary>
    /// Найдет следующий компонент выполнения.
    /// </summary>
    /// <param name="elementsSrc">Весь список доступных нод.</param>
    /// <param name="currentNodes">Текущие ноды.</param>
    /// <param name="context">Контекст операции, для шлюзов.</param>
    /// <returns>Вернет компоненты соответсвующее критерию поиска.</returns>
    public IElement[] GetNextNode(IElement[]? elementsSrc, IElement[]? currentNodes, IExclusiveGateWayRoute context);

    /// <summary>
    /// Поиск путей выполнение для ExclusiveGateWay.
    /// </summary>
    /// <param name="context">Контекст операции.</param>
    /// <param name="currentNode">Текущая нода.</param>
    /// <returns>Flow следущие ноды.</returns>
    public string GetConditionRouteWithExclusiveGateWay(IExclusiveGateWayRoute context, IElement currentNode);
}