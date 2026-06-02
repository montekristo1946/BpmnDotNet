using System.Collections.Concurrent;
using BpmnDotNet.BpmnEngineDomain.Abstractions;

namespace BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Описывает модель bpmn процесса.
/// </summary>
internal class ProcessModel
{
    /// <summary>
    /// Список нод.
    /// </summary>
    public ConcurrentDictionary<string, IBpmnNode> Nodes { get; set; } = new();

    /// <summary>
    /// Массив стрелочек.
    /// </summary>
    public ConcurrentQueue<Flow> Flow { get; set; } = new();
}