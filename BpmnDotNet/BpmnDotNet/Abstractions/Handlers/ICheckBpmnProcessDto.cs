namespace BpmnDotNet.Abstractions.Handlers;

using BpmnDotNet.Abstractions.Elements;

/// <summary>
/// Проверка Bpmn в объектном виде.
/// </summary>
internal interface ICheckBpmnProcessDto
{
    /// <summary>
    /// Запустить проверку.
    /// </summary>
    /// <param name="bpmnProcess">BpmnProcessDto.</param>
    public void Check(BpmnProcessDto bpmnProcess);
}