using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BpmnDotNet.Common.Models;

public enum ElementType
{
    [Display(Name = "Не заданно")]
    [Description("None")]
    None = 0,

    [Display(Name = "startEvent")]
    [Description("StartEvent")]
    StartEvent = 1,

    [Display(Name = "endEvent")]
    [Description("EtartEvent")]
    EndEvent = 2,

    [Display(Name = "sequenceFlow")]
    [Description("sequenceFlow")]
    SequenceFlow = 3,

    [Display(Name = "exclusiveGateway")]
    [Description("exclusiveGateway")]
    ExclusiveGateway = 4,

    [Display(Name = "parallelGateway")]
    [Description("parallelGateway")]
    ParallelGateway = 5,

    [Display(Name = "serviceTask")]
    [Description("serviceTask")]
    ServiceTask = 6,

    [Display(Name = "sendTask")]
    [Description("sendTask")]
    SendTask = 7,

    [Display(Name = "receiveTask")]
    [Description("receiveTask")]
    ReceiveTask = 8,

    [Display(Name = "subProcess")]
    [Description("subProcess")]
    SubProcess = 9
}