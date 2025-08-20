using System.Threading;
using System.Threading.Tasks;

namespace BpmnDotNet.Common.Abstractions;

/// <summary>
/// Интерфейс обработчиков блоков Bpmn.
/// </summary>
public interface IBpmnHandler
{
    /// <summary>
    /// ID блочка в Bpmn нотации.
    /// </summary>
    string TaskDefinitionId { get; init; }

    /// <summary>
    /// Метод, который вызовет Broker.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken = default);


}