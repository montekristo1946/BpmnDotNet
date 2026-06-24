namespace BpmnDotNet.BpmnEngineDomain.Dto;

using System.Collections.Concurrent;
using BpmnDotNet.BpmnEngineDomain.Abstractions;

/// <summary>
/// Описывает модель bpmn процесса.
/// </summary>
internal record ProcessModel
{
    /// <summary>
    /// Gets список нод.
    /// </summary>
    public ConcurrentDictionary<string, IBpmnNode> Nodes { get; init; } = new();

    /// <summary>
    /// Gets основное хранилище стрелочек.
    /// </summary>
    public ConcurrentDictionary<string, Flow> Flows { get; init; } = new();

    /// <summary>
    /// Gets индексы для быстрого поиска source, (ID целевого, ID flow выходные).
    /// </summary>
    public ConcurrentDictionary<string, DirectionFlow[]> FlowsBySource { get; init; } = new();

    /// <summary>
    /// Gets индексы для быстрого поиска Target, (ID целевого, ID flow входные).
    /// </summary>
    public ConcurrentDictionary<string, DirectionFlow[]> FlowsByTarget { get; init; } = new();
}
