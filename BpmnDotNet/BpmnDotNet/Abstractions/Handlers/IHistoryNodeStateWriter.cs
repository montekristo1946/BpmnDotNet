using System.Collections.Concurrent;
using BpmnDotNet.Common.Dto;
using BpmnDotNet.Dto;

namespace BpmnDotNet.Abstractions.Handlers;

internal interface IHistoryNodeStateWriter
{
    Task SetStateProcess(
        string idBpmnProcess,
        string tokenProcess,
        NodeJobStatus[] nodeStateRegistry,
        string[] arrayMessageErrors,
        bool isCompleted,
        long dateFromInitInstance);
}