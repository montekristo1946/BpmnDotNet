using System.Runtime.CompilerServices;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("BpmnDotNetTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Handlers;

internal static class BpmnClientBuilder
{
    public static IBpmnClient Build(string pathDiagram,
        ILoggerFactory loggerFactory,
        IPathFinder pathFinder,
        IElasticClient elasticClient,
        IHistoryNodeStateWriter historyNodeStateWriter)
    {
        var allBpmnFiles = GetAllFiles(pathDiagram);
        var businessProcessDtos = CreateBusinessProcessDtos(allBpmnFiles);
        LoadBpmnInElastic(allBpmnFiles,elasticClient);
        return new BpmnClient(businessProcessDtos, loggerFactory, pathFinder,historyNodeStateWriter);
    }

    private static void LoadBpmnInElastic(string[] allBpmnFiles, IElasticClient elasticClient)
    {
        var xmlBpmnLoader = new XmlSerializationBpmnDiagramSection();
        foreach (var allBpmnFile in allBpmnFiles)
        {
           var plane= xmlBpmnLoader.LoadXmlBpmnDiagram(allBpmnFile);
           var res =elasticClient.SetDataAsync(plane).Result;
           if (!res)
           {
               throw new InvalidOperationException($"Failed to set data for file {allBpmnFile}");
           }
        }
    }

    private static BpmnProcessDto[] CreateBusinessProcessDtos(string[] allBpmnFiles)
    {
        var xmlBpmnLoader = new XmlSerializationProcessSection();

        var retValue = allBpmnFiles.Select(p =>
            xmlBpmnLoader.LoadXmlProcessSection(p)).ToArray();

        return retValue;
    }

    private static string[] GetAllFiles(string pathDiagram)
    {
        var searchPattern = "*.bpmn";
        var files = Directory.GetFiles(pathDiagram, searchPattern);
        return files;
    }
}