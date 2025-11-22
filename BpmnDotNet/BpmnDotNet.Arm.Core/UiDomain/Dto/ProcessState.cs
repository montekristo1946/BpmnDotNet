namespace BpmnDotNet.Arm.Core.UiDomain.Dto;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
///  Статусы процессов Для АРМа.
/// </summary>
public enum ProcessState
{
    /// <summary>
    /// Не определенное состояние.
    /// </summary>
    [Display(Name = "Не заданно")]
    [Description("None")]
    None = 0,

    /// <summary>
    /// Сейчас работает.
    /// </summary>
    [Display(Name = "Running")]
    [Description("Running")]
    Works = 1,

    /// <summary>
    /// Удачно завершен.
    /// </summary>
    [Display(Name = "Completed")]
    [Description("Completed")]
    Completed = 2,

    /// <summary>
    /// Не удачно завершен.
    /// </summary>
    [Display(Name = "Error")]
    [Description("Error")]
    Error = 3,
}