namespace BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Состояния BpmnEngine.
/// </summary>
public enum StatusNode
{
    /// <summary>
    ///     Не определенное состояние.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Сейчас выполняется.
    /// </summary>
    WorksNode = 1,

    /// <summary>
    ///     Удачно завершен.
    /// </summary>
    CompletedNode = 2,

    /// <summary>
    ///     Не удачно завершен.
    /// </summary>
    FailedNode = 3,

    /// <summary>
    /// Завершен весь процесс bpmn.
    /// </summary>
    AllBpmnProcessCompleted = 4,
}