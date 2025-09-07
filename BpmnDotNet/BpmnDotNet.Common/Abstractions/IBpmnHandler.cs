namespace BpmnDotNet.Common.Abstractions;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     Интерфейс обработчиков блоков Bpmn.
/// </summary>
public interface IBpmnHandler
{
    /// <summary>
    ///     Gets Id блочка в Bpmn нотации.
    /// </summary>
    string TaskDefinitionId { get; init; }

    /// <summary>
    ///     Метод, который вызовет Broker.
    /// </summary>
    /// <param name="context">IContextBpmnProcess.</param>
    /// <param name="ctsToken">CancellationToken.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task AsyncJobHandler(IContextBpmnProcess context, CancellationToken ctsToken = default);
}