using System.Collections.Concurrent;
using BpmnDotNet.Dto;

namespace BpmnDotNet.Abstractions.Handlers;

internal interface IHistoryNodeStateWriter
{
    Task SetStateProcess(
        string idBpmnProcess,
        string tokenProcess,
        NodeTaskStatus[] nodeStateRegistry,
        string[] arrayMessageErrors,
        bool isCompleted,
        long dateFromInitInstance);
}