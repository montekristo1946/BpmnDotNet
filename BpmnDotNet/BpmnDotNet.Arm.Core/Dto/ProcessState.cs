using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BpmnDotNet.Arm.Core.Dto;

public enum ProcessState
{
    [Display(Name = "Не заданно")] [Description("None")]
    None = 0,

    [Display(Name = "Running")] [Description("Running")]
    Running = 1,

    [Display(Name = "Completed")] [Description("Completed")]
    Completed = 2,
    
    [Display(Name = "Error")] [Description("Error")]
    Error = 3,
}