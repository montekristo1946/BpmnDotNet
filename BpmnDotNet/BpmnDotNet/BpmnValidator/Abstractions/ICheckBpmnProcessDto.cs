using BpmnDotNet.Dto;

namespace BpmnDotNet.BpmnValidator.Abstractions;

using BpmnDotNet.BpmnEngineDomain.Dto;

/// <summary>
/// Проверка Bpmn в объектном виде.
/// </summary>
internal interface ICheckBpmnProcessDto
{
    /// <summary>
    /// Запустить проверку.
    /// </summary>
    /// <param name="bpmnProcess"><see cref="BpmnProcessDto"/>.</param>
    public void Check(BpmnProcessDto bpmnProcess);
}