using BpmnDotNet.Abstractions.Elements;

namespace BpmnDotNet.Abstractions.Handlers;

public interface ICheckBpmnProcessDto
{
    void Check(BpmnProcessDto bpmnProcess);
}