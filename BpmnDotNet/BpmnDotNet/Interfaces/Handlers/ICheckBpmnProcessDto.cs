using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Interfaces.Handlers;

public interface ICheckBpmnProcessDto
{
    void Check(BpmnProcessDto bpmnProcess);
}