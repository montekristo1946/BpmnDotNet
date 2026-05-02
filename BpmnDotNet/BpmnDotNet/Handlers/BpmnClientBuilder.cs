using System.Runtime.CompilerServices;
using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("BpmnDotNetTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace BpmnDotNet.Handlers;

/// <summary>
/// BpmnClientBuilder.
/// </summary>
internal static class BpmnClientBuilder
{
    /// <summary>
    /// Build IBpmnClient.
    /// </summary>
    /// <param name="pathDiagram">Путь до диаграмм.</param>
    /// <param name="loggerFactory">ILoggerFactory.</param>
    /// <param name="pathFinder">IPathFinder.</param>
    /// <param name="elasticClient">IElasticClientSetDataAsync.</param>
    /// <param name="historyNodeStateWriter">IHistoryNodeStateWriter.</param>
    /// <param name="descriptionWriteService">IDescriptionWriteService.</param>
    /// <param name="serializerProcessSection">IXmlSerializationProcessSection.</param>
    /// <param name="serializerDiagramSection">IXmlSerializationBpmnDiagramSection.</param>
    /// <returns>IBpmnClient.</returns>
    public static IBpmnClient Build(
        string pathDiagram,
        ILoggerFactory loggerFactory,
        IPathFinder pathFinder,
        IElasticClientSetDataAsync elasticClient,
        IHistoryNodeStateWriter historyNodeStateWriter,
        IDescriptionWriteService descriptionWriteService,
        IXmlSerializationProcessSection serializerProcessSection,
        IXmlSerializationBpmnDiagramSection serializerDiagramSection)
    {
        ArgumentNullException.ThrowIfNull(pathDiagram);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(pathFinder);
        ArgumentNullException.ThrowIfNull(elasticClient);
        ArgumentNullException.ThrowIfNull(historyNodeStateWriter);
        ArgumentNullException.ThrowIfNull(descriptionWriteService);
        ArgumentNullException.ThrowIfNull(serializerProcessSection);
        ArgumentNullException.ThrowIfNull(serializerDiagramSection);

        var allBpmnFiles = GetAllFiles(pathDiagram);
        var businessProcessDtos = allBpmnFiles.Select(serializerProcessSection.LoadXmlProcessSection).ToArray();
        LoadBpmnInElastic(allBpmnFiles, elasticClient, serializerDiagramSection);
        return new BpmnClient(
            businessProcessDtos,
            loggerFactory,
            pathFinder,
            historyNodeStateWriter,
            descriptionWriteService);
    }

    /// <summary>
    /// Получить файлы bpmn.
    /// </summary>
    /// <param name="pathDiagram">Путь до диаграм.</param>
    /// <returns>Диаграммы в формате string[].</returns>
    internal static string[] GetAllFiles(string pathDiagram)
    {
        if (string.IsNullOrWhiteSpace(pathDiagram))
        {
            throw new ArgumentException("[GetAllFiles] Path cannot be null or whitespace.", nameof(pathDiagram));
        }

        if (!Directory.Exists(pathDiagram))
        {
            throw new DirectoryNotFoundException($"[GetAllFiles] Directory not found: {pathDiagram}");
        }

        var searchPattern = "*.bpmn";
        var files = Directory.GetFiles(pathDiagram, searchPattern);

        return files.Length == 0
            ? throw new InvalidOperationException(
                $"[GetAllFiles] Not found files in diagram {pathDiagram}:{searchPattern}.")
            : files;
    }

    private static void LoadBpmnInElastic(
        string[] allBpmnFiles,
        IElasticClientSetDataAsync elasticClient,
        IXmlSerializationBpmnDiagramSection serializer)
    {
        foreach (var allBpmnFile in allBpmnFiles)
        {
            var plane = serializer.LoadXmlBpmnDiagram(allBpmnFile);
            var res = elasticClient.SetDataAsync(plane).Result;
            if (!res)
            {
                throw new InvalidOperationException($"Failed to set data for file {allBpmnFile}");
            }
        }
    }
}