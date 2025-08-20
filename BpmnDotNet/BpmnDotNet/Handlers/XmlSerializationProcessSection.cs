using System.Xml;
using BpmnDotNet.Elements.BpmnNatation;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;


namespace BpmnDotNet.Handlers;

internal class XmlSerializationProcessSection : IXmlSerializationProcessSection
{
    public BpmnProcessDto LoadXmlProcessSection(string pathDiagram)
    {
        if (string.IsNullOrWhiteSpace(pathDiagram))
            throw new ArgumentNullException(nameof(pathDiagram));
        
        var ret = new XmlDocument();
        ret.Load(pathDiagram);

        var retValue = ParsingXml(ret);
        return retValue;
    }

    private BpmnProcessDto ParsingXml(XmlDocument xmlDoc)
    {
        ArgumentNullException.ThrowIfNull(xmlDoc);

        XmlNode? root = xmlDoc.DocumentElement;
        if (root == null || root.Name != Constants.BpmnRootName)
            throw new InvalidOperationException($"Not find {Constants.BpmnRootName}");


        var processBlock = root.ChildNodes.Cast<XmlNode>().FirstOrDefault(p => p.Name == Constants.BpmnProcessName);
        if (processBlock is null)
            throw new InvalidOperationException($"Not find {Constants.BpmnProcessName}");


        var elements = new List<IElement>();
        var idProcess = GetId(processBlock);

        foreach (XmlNode xmlNode in processBlock.ChildNodes)
        {
            var element = xmlNode.Name switch
            {
                Constants.BpmnStartEventName => CreateStartEvent(xmlNode),
                Constants.BpmnSequenceFlowName => CreateSequenceFlow(xmlNode),
                Constants.BpmnEndEventName => CreateEndEvent(xmlNode),
                Constants.BpmnExclusiveGatewayName => CreateExclusiveGateway(xmlNode),
                Constants.BpmnParallelGatewayName => CreateParallelGateway(xmlNode),
                Constants.BpmnSendTaskName => CreateSendTaskName(xmlNode),
                Constants.BpmnReceiveTaskName => CreateReceiveTask(xmlNode),
                Constants.BpmnServiceTaskName => CreateServiceTask(xmlNode),
                Constants.BpmnSubProcess => CreateSubProcess(xmlNode),
                _ => throw new ArgumentOutOfRangeException($"{idProcess} { xmlNode.Name}")
            };
            elements.Add(element);
        }

        if (elements.Any() is false)
            throw new InvalidOperationException($"Not find elements in {processBlock.Name}");

        return new BpmnProcessDto(idProcess, elements.ToArray());
    }

    private IElement CreateSubProcess(XmlNode elements)
    {
        var id = GetId(elements);
        var outgoing = GetOutGoing(elements,id);
        var incoming = GetIncoming(elements,id);

        return new SubProcessComponent(id, incoming, outgoing);
    }

    private ServiceTaskComponent CreateServiceTask(XmlNode elements)
    {
        var id = GetId(elements);
        var outgoing = GetOutGoing(elements,id);
        var incoming = GetIncoming(elements,id);

        return new ServiceTaskComponent(id, incoming, outgoing);
    }

    private ReceiveTaskComponent CreateReceiveTask(XmlNode elements)
    {
        var id = GetId(elements);
        var outgoing = GetOutGoing(elements,id);
        var incoming = GetIncoming(elements,id);

        return new ReceiveTaskComponent(id, incoming, outgoing);
    }

    private SendTaskComponent CreateSendTaskName(XmlNode elements)
    {
        var id = GetId(elements);
        var outgoing = GetOutGoing(elements,id);
        var incoming = GetIncoming(elements,id);

        return new SendTaskComponent(id, incoming, outgoing);
    }

    private ParallelGatewayComponent CreateParallelGateway(XmlNode elements)
    {
        var id = GetId(elements);
        var outgoing = GetOutGoing(elements,id);
        var incoming = GetIncoming(elements,id);

        return new ParallelGatewayComponent(id, incoming, outgoing);
    }

    private ExclusiveGatewayComponent CreateExclusiveGateway(XmlNode elements)
    {
        var id = GetId(elements);
        var outgoing = GetOutGoing(elements,id);
        var incoming = GetIncoming(elements,id);

        return new ExclusiveGatewayComponent(id, incoming, outgoing);
    }


    private SequenceFlowComponent CreateSequenceFlow(XmlNode elements)
    {
        var id = GetId(elements);
        var incoming = elements.Attributes?[Constants.BpmnSourceRef]?.Value
                       ?? throw new InvalidDataException($"{id} Not Find targetRef from:{elements.Name}");

        var outGoing = elements.Attributes?[Constants.BpmnTargetRef]?.Value
                       ?? throw new InvalidDataException($"{id} Not Find sourceRef from:{elements.Name}");

        var sequenceFlow = new SequenceFlowComponent(id, [incoming], [outGoing]);
        return sequenceFlow;
    }

    private StartEventComponent CreateStartEvent(XmlNode elements)
    {
        var id = GetId(elements);
        var outgoing = GetOutGoing(elements,id);
        var startEvent = new StartEventComponent(id, outgoing);

        return startEvent;
    }

    private EndEventComponent CreateEndEvent(XmlNode elements)
    {
        var id = GetId(elements);
        var incoming = GetIncoming(elements,id);
        var endEvent = new EndEventComponent(id, incoming);

        return endEvent;
    }

    private string[] GetIncoming(XmlNode elements, string idNode)
    {
        var outgoing = elements.ChildNodes.Cast<XmlNode>()
            .Where(p => p.Name == Constants.BpmnIncomingName)
            .Select(p => p.InnerText)
            .ToArray();

        if (outgoing.Any() is false)
            throw new InvalidDataException($"{idNode} Not Find Incoming from:{elements.Name}");

        return outgoing;
    }

    private string[] GetOutGoing(XmlNode elements, string idNode)
    {
        var outgoing = elements.ChildNodes.Cast<XmlNode>()
            .Where(p => p.Name == Constants.BpmnOutGoingName)
            .Select(p => p.InnerText)
            .ToArray();

        if (outgoing.Any() is false)
            throw new InvalidDataException($"{idNode} Not Find outgoing from:{elements.Name}");

        return outgoing;
    }

    private string GetId(XmlNode elements)
    {
        var id = elements.Attributes?[Constants.BpmnIdName]?.Value
                 ?? throw new InvalidOperationException($"Not Find ID from:{elements.Name}");
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidDataException($"Not Find ID from:{elements.Name}");

        return id;
    }
}