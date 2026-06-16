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
    Works = 1,

    /// <summary>
    ///     Удачно завершен.
    /// </summary>
    NormalCompleted = 2,

    /// <summary>
    ///     Не удачно завершен.
    /// </summary>
    FailedCompleted = 3,

    /// <summary>
    /// Завершен весь процесс bpmn.
    /// </summary>
    AllBpmnProcessCompleted = 4,
}