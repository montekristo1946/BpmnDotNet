using BpmnDotNet.Common.Abstractions;
using Sample.ConsoleApp.Common;

namespace Sample.ConsoleApp.Context;

public class ContextSubProcess : IContextBpmnProcess
{
    public string ContextSubProcessValue { get; set; } = string.Empty;
    public string IdBpmnProcess { get; init; } = Constants.IdBpmnProcessingSub;
    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();
}