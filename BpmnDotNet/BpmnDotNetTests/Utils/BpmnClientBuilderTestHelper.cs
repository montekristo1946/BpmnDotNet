namespace BpmnDotNetTests.Utils;

internal static class BpmnClientBuilderTestHelper
{
    internal static string[] CreateSampleBpmnFiles(int count, string directory)
    {
        var files = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var fileName = $"process_{i + 1}_{Guid.NewGuid()}.bpmn";
            var filePath = Path.Combine(directory, fileName);
            var content = CreateValidBpmnContent($"process_{i + 1}");
            File.WriteAllText(filePath, content);
            files.Add(filePath);
        }

        return files.ToArray();
    }
    
    internal static void CreateOtherFiles(int count,string directory, string extension = "txt")
    {
        for (int i = 0; i < count; i++)
        {
            var fileName = $"file_{i + 1}_{Guid.NewGuid()}.{extension}";
            var filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, $"Test content {i}");
        }
    }

    internal static string CreateValidBpmnContent(string processId)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
            <definitions xmlns=""http://www.omg.org/spec/BPMN/20100524/MODEL""
                         xmlns:bpmndi=""http://www.omg.org/spec/BPMN/20100524/DI""
                         xmlns:dc=""http://www.omg.org/spec/DD/20100524/DC""
                         xmlns:di=""http://www.omg.org/spec/DD/20100524/DI""
                         id=""Definitions_{processId}""
                         targetNamespace=""http://bpmn.io/schema/bpmn"">
                <process id=""{processId}"" name=""{processId}"" isExecutable=""true"">
                    <startEvent id=""StartEvent_1"" name=""Start"" />
                    <endEvent id=""EndEvent_1"" name=""End"" />
                    <sequenceFlow id=""Flow_1"" sourceRef=""StartEvent_1"" targetRef=""EndEvent_1"" />
                </process>
                <bpmndi:BPMNDiagram id=""BPMNDiagram_{processId}"">
                    <bpmndi:BPMNPlane id=""BPMNPlane_{processId}"" bpmnElement=""{processId}"">
                        <bpmndi:BPMNShape id=""Shape_StartEvent_1"" bpmnElement=""StartEvent_1"">
                            <dc:Bounds x=""100"" y=""100"" width=""36"" height=""36"" />
                        </bpmndi:BPMNShape>
                        <bpmndi:BPMNShape id=""Shape_EndEvent_1"" bpmnElement=""EndEvent_1"">
                            <dc:Bounds x=""300"" y=""100"" width=""36"" height=""36"" />
                        </bpmndi:BPMNShape>
                        <bpmndi:BPMNEdge id=""Edge_Flow_1"" bpmnElement=""Flow_1"">
                            <di:waypoint x=""136"" y=""118"" />
                            <di:waypoint x=""300"" y=""118"" />
                        </bpmndi:BPMNEdge>
                    </bpmndi:BPMNPlane>
                </bpmndi:BPMNDiagram>
            </definitions>";
    }

}