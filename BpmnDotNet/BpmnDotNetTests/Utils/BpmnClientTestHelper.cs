using System.Collections.Concurrent;
using System.Reflection;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Dto;
using BpmnDotNet.Handlers;

namespace BpmnDotNetTests.Utils;

internal static class BpmnClientTestHelper
{
    internal static ConcurrentDictionary<(string IdBpmnProcess, string TokenProcess), BusinessProcessJobStatus> GetBpmnProcessesDictionary( BpmnClient bpmnClient)
    {
        var field = typeof(BpmnClient)
            .GetField("BpmnProcesses", BindingFlags.NonPublic | BindingFlags.Instance);
        
        return field?.GetValue(bpmnClient) as ConcurrentDictionary<(string, string), BusinessProcessJobStatus>
               ?? throw new InvalidOperationException("Cannot get BpmnProcesses field");
    }
    
    internal static void AddProcessToStore(string idBpmnProcess, string tokenProcess, IBusinessProcess? process, BpmnClient bpmnClient)
    {
        var processes = GetBpmnProcessesDictionary(bpmnClient);
        
        var jobStatus = new BusinessProcessJobStatus
        {
            StatusType = StatusType.Works,
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess,
            Process = process!
        };
        
        processes[(idBpmnProcess, tokenProcess)] = jobStatus;
    }
}