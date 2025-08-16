namespace BpmnDotNet.Interfaces.Handlers;

/// <summary>
/// Интерфейс обработчиков блоков Bpmn.
/// </summary>
public interface IBpmnHandler
{
    /// <summary>
    /// ID блочка в Bpmn натации.
    /// </summary>
    string TaskDefinitionId { get; init; }

    /// <summary>
    /// Метод, который вызовет Broker.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken = default);


}