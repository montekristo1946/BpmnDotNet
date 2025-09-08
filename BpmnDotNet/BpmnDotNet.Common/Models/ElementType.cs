namespace BpmnDotNet.Common.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Тип блока.
/// </summary>
public enum ElementType
{
    /// <summary>
    /// Не определено.
    /// </summary>
    [Display(Name = "Не заданно")]
    [Description("None")]
    None = 0,

    /// <summary>
    /// Блок Старт процесса.
    /// </summary>
    [Display(Name = "startEvent")]
    [Description("StartEvent")]
    StartEvent = 1,

    /// <summary>
    /// Окончание процесса.
    /// </summary>
    [Display(Name = "endEvent")]
    [Description("EtartEvent")]
    EndEvent = 2,

    /// <summary>
    /// Маршрутные стрелочки  на bpmn.
    /// </summary>
    [Display(Name = "sequenceFlow")]
    [Description("sequenceFlow")]
    SequenceFlow = 3,

    /// <summary>
    /// Эксклюзивный шлюз.
    /// </summary>
    [Display(Name = "exclusiveGateway")]
    [Description("exclusiveGateway")]
    ExclusiveGateway = 4,

    /// <summary>
    /// Параллельный шлюз.
    /// </summary>
    [Display(Name = "parallelGateway")]
    [Description("parallelGateway")]
    ParallelGateway = 5,

    /// <summary>
    /// Сервис таск.
    /// </summary>
    [Display(Name = "serviceTask")]
    [Description("serviceTask")]
    ServiceTask = 6,

    /// <summary>
    /// Таск отправки сообщений.
    /// </summary>
    [Display(Name = "sendTask")]
    [Description("sendTask")]
    SendTask = 7,

    /// <summary>
    /// Таск приема сообщений.
    /// </summary>
    [Display(Name = "receiveTask")]
    [Description("receiveTask")]
    ReceiveTask = 8,

    /// <summary>
    /// СубПроцесс.
    /// </summary>
    [Display(Name = "subProcess")]
    [Description("subProcess")]
    SubProcess = 9,
}