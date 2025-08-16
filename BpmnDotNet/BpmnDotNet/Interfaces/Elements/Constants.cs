namespace BpmnDotNet.Interfaces.Elements;

internal static class Constants
{
    public const string BpmnRootName = "bpmn:definitions";
    public const string BpmnProcessName = "bpmn:process";
    public const string BpmnStartEventName = "bpmn:startEvent";
    public const string BpmnEndEventName = "bpmn:endEvent";

    public const string BpmnOutGoingName = "bpmn:outgoing";
    public const string BpmnIncomingName = "bpmn:incoming";

    public const string BpmnSequenceFlowName = "bpmn:sequenceFlow";

    public const string BpmnExclusiveGatewayName = "bpmn:exclusiveGateway";
    public const string BpmnParallelGatewayName = "bpmn:parallelGateway";

    public const string BpmnSendTaskName = "bpmn:sendTask";
    public const string BpmnReceiveTaskName = "bpmn:receiveTask";
    public const string BpmnServiceTaskName = "bpmn:serviceTask";

    public const string BpmnSubProcess = "bpmn:subProcess";

    public const string BpmnTargetRef = "targetRef";
    public const string BpmnIdName = "id";
    public const string BpmnSourceRef = "sourceRef";

}