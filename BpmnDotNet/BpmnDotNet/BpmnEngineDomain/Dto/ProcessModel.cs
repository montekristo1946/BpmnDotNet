namespace BpmnDotNet.BpmnEngineDomain.Dto;

using System.Collections.Concurrent;
using BpmnDotNet.BpmnEngineDomain.Abstractions;

/// <summary>
/// Описывает модель bpmn процесса.
/// </summary>
internal class ProcessModel
{
    /// <summary>
    /// Gets список нод.
    /// </summary>
    public ConcurrentDictionary<string, IBpmnNode> Nodes { get; init; } = new();

    /// <summary>
    /// Gets массив стрелочек.
    /// </summary>
    public ConcurrentQueue<Flow> Flow { get; init; } = new();
}