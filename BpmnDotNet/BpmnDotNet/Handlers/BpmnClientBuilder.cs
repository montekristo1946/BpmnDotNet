using System.Runtime.CompilerServices;
using BpmnDotNet.Interfaces.Elements;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("BpmnDotNetTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Handlers;

internal static class BpmnClientBuilder
{
    public static IBpmnClient Build(string pathDiagram,
        ILoggerFactory loggerFactory,
        IPathFinder pathFinder)
    {
        var allBpmnFiles = GetAllFiles(pathDiagram);
        var businessProcessDtos = CreateBusinessProcessDtos(allBpmnFiles);

        return new BpmnClient(businessProcessDtos, loggerFactory, pathFinder);
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