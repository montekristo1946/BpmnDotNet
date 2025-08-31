namespace BpmnDotNet.Arm.Core.Dto;

public class ListProcessPanelDto
{
    /// <summary>
    ///     ID Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; } = string.Empty;

    /// <summary>
    ///     Время создания документа.
    /// </summary>
    public DateTime DateCreated { get; init; } = DateTime.Now;
    
    /// <summary>
    ///     Время последнего изменения
    /// </summary>
    public DateTime DateLastModified { get; init; } = DateTime.Now;


    /// <summary>
    ///     Состояние процесса.
    /// </summary>
    public ProcessState State { get; init; } = ProcessState.None;
    
    
    /// <summary>
    ///     IdStorageHistoryNodeState.
    /// </summary>
    public string IdStorageHistoryNodeState { get; init; } = string.Empty;
}