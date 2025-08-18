using System.Collections.Concurrent;
using BpmnDotNet.Common;
using BpmnDotNet.Common.Interfases;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;

namespace Sample.ConsoleApp.Context;

public class ContextSubProcess : IContextBpmnProcess
{
    public string IdBpmnProcess { get; init; } = Common.Constants.IdBpmnProcessingSub;
    public string TokenProcess { get; init; } = Guid.NewGuid().ToString();

    public string ContextSubProcessValue { get; set; } = string.Empty;
}