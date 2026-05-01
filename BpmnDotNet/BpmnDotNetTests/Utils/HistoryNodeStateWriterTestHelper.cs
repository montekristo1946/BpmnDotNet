using BpmnDotNet.Dto;

namespace BpmnDotNetTests.Utils;

internal static class HistoryNodeStateWriterTestHelper
{
    public static IEnumerable<object[]> GetNodeStateScenarios()
    {
        yield return
        [
            new[] { new NodeJobStatus { IdNode = "1", StatusType = StatusType.Works } },
            false,
            ProcessStatus.Works
        ];
        
        yield return
        [
            new[] 
            { 
                new NodeJobStatus { IdNode = "1", StatusType = StatusType.Completed },
                new NodeJobStatus { IdNode = "2", StatusType = StatusType.Completed }
            },
            true,
            ProcessStatus.Completed
        ];
        
        yield return
        [
            new[] 
            { 
                new NodeJobStatus { IdNode = "1", StatusType = StatusType.Completed },
                new NodeJobStatus { IdNode = "2", StatusType = StatusType.Failed }
            },
            false,
            ProcessStatus.Error
        ];
    }
}