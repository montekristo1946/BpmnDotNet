namespace BpmnDotNet.Interfaces.Handlers;

public interface IContextBpmnProcess
{

    /// <summary>
    /// ID Bpmn процесса из схемы bpmn.
    /// </summary>
    public string IdBpmnProcess { get; init; }

    /// <summary>
    /// Текущий Token процесса.
    /// </summary>
    public string TokenProcess { get; init; }


}