using BpmnDotNet.Interfaces.Elements;

namespace BpmnDotNet.Interfaces.Handlers;

public interface IXmlBpmnLoader
{
    BpmnProcessDto? LoadXml(string pathDiagram);
}