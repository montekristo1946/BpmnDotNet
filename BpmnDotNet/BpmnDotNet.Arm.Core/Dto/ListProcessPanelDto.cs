namespace BpmnDotNet.Arm.Core.Dto;

public class ListProcessPanelDto
{
    /// <summary>
    ///     ID Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; set; } = string.Empty;

    /// <summary>
    ///     Текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; set; } = string.Empty;

    /// <summary>
    ///     Время создания документа.
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.Now;
    
    /// <summary>
    ///     Время последнего изменения
    /// </summary>
    public DateTime DateLastModified { get; set; } = DateTime.Now;


    /// <summary>
    ///     Состояние процесса.
    /// </summary>
    public ProcessState State { get; set; } = ProcessState.None;
}