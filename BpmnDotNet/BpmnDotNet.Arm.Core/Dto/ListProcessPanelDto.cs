namespace BpmnDotNet.Arm.Core.Dto;

/// <summary>
/// ДТО для панели ListProcess.
/// </summary>
public class ListProcessPanelDto
{
    /// <summary>
    ///     Gets iD Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Gets текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Gets время создания документа.
    /// </summary>
    public DateTime DateCreated { get; init; } = DateTime.Now;

    /// <summary>
    ///     Gets время последнего изменения.
    /// </summary>
    public DateTime DateLastModified { get; init; } = DateTime.Now;

    /// <summary>
    ///     Gets состояние процесса.
    /// </summary>
    public ProcessState State { get; init; } = ProcessState.None;

    /// <summary>
    ///     Gets idStorageHistoryNodeState.
    /// </summary>
    public string IdStorageHistoryNodeState { get; init; } = string.Empty;
}