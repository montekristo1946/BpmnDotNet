using System.Collections.Concurrent;
using AutoFixture.Xunit2;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.Dto;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using BpmnDotNet.Handlers;
using BpmnDotNet.HistoryDomain.Dto;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.HistoryDomain;

public class HistoryNodeStateWriterTest
{
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldSaveHistoryState_WhenSuccess(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        nodeStateRegistry.TryAdd("Node2", StatusNode.AllBpmnProcessCompleted);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        errorRegistry.TryAdd("Error1", "Test error message");
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.IdBpmnProcess == idBpmnProcess &&
                state.TokenProcess == tokenProcess &&
                state.DateCreated == dateFromInitInstance &&
                state.NodeStaus.Length == 2 &&
                state.ArrayMessageErrors.Length == 1));
        
    }
    
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldCalculateProcessStatusAsError_WhenFailedNodeExists(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        nodeStateRegistry.TryAdd("Node2", StatusNode.FailedCompleted);
        nodeStateRegistry.TryAdd("Node3", StatusNode.Works);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.ProcessStatus == ProcessStatus.Error));
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldCalculateProcessStatusAsWorks_WhenWorksNodeExists(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        nodeStateRegistry.TryAdd("Node2", StatusNode.Works);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.ProcessStatus == ProcessStatus.Works));
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldCalculateProcessStatusAsCompleted_WhenAllCompleted(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        nodeStateRegistry.TryAdd("Node2", StatusNode.AllBpmnProcessCompleted);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.ProcessStatus == ProcessStatus.Completed));
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldCalculateProcessStatusAsNone_WhenNoMatchingStatus(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.None);
        nodeStateRegistry.TryAdd("Node2", StatusNode.None);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.ProcessStatus == ProcessStatus.None));
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldLogError_WhenElasticClientFails(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(false);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[HistoryNodeStateWriter:SetStateProcessAsync] Failed to set history node state")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentNullException_WhenNodeStateRegistryIsNull(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SetStateProcessAsync(
                idBpmnProcess,
                tokenProcess,
                null!,
                errorRegistry,
                dateFromInitInstance));
    }
    
     [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentNullException_WhenErrorRegistryIsNull(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SetStateProcessAsync(
                idBpmnProcess,
                tokenProcess,
                nodeStateRegistry,
                null!,
                dateFromInitInstance));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentNullException_WhenIdBpmnProcessIsNull(
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SetStateProcessAsync(
                null!,
                tokenProcess,
                nodeStateRegistry,
                errorRegistry,
                dateFromInitInstance));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentNullException_WhenIdBpmnProcessIsEmpty(
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SetStateProcessAsync(
                "",
                tokenProcess,
                nodeStateRegistry,
                errorRegistry,
                dateFromInitInstance));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentNullException_WhenIdBpmnProcessIsWhitespace(
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SetStateProcessAsync(
                "   ",
                tokenProcess,
                nodeStateRegistry,
                errorRegistry,
                dateFromInitInstance));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentNullException_WhenTokenProcessIsNull(
        string idBpmnProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SetStateProcessAsync(
                idBpmnProcess,
                null!,
                nodeStateRegistry,
                errorRegistry,
                dateFromInitInstance));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentNullException_WhenTokenProcessIsEmpty(
        string idBpmnProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SetStateProcessAsync(
                idBpmnProcess,
                "",
                nodeStateRegistry,
                errorRegistry,
                dateFromInitInstance));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldThrowArgumentOutOfRangeException_WhenDateFromInitInstanceIsNegative(
        string idBpmnProcess,
        string tokenProcess,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        var errorRegistry = new ConcurrentDictionary<string, string>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.SetStateProcessAsync(
                idBpmnProcess,
                tokenProcess,
                nodeStateRegistry,
                errorRegistry,
                -1));
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldSetCorrectDateLastModified(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        
        var beforeCall = DateTime.Now.Ticks;
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.DateLastModified >= beforeCall &&
                state.DateLastModified <= DateTime.Now.Ticks));
    }
    
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldIncludeAllErrorsFromRegistry(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        errorRegistry.TryAdd("Error1", "First error");
        errorRegistry.TryAdd("Error2", "Second error");
        errorRegistry.TryAdd("Error3", "Third error");
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.ArrayMessageErrors.Length == 3 &&
                state.ArrayMessageErrors.Contains("First error") &&
                state.ArrayMessageErrors.Contains("Second error") &&
                state.ArrayMessageErrors.Contains("Third error")));
    }
    
    [Theory]
    [AutoNSubstituteData]
    internal async Task SetStateProcessAsync_ShouldIncludeAllNodesFromRegistry(
        string idBpmnProcess,
        string tokenProcess,
        long dateFromInitInstance,
        [Frozen] IElasticClientSetDataAsync elasticClient,
        [Frozen] ILogger<HistoryNodeStateWriter> logger)
    {
        // Arrange
        var sut = new HistoryNodeStateWriter(elasticClient, logger);
        
        var nodeStateRegistry = new ConcurrentDictionary<string, StatusNode>();
        nodeStateRegistry.TryAdd("Node1", StatusNode.NormalCompleted);
        nodeStateRegistry.TryAdd("Node2", StatusNode.Works);
        nodeStateRegistry.TryAdd("Node3", StatusNode.FailedCompleted);
        
        var errorRegistry = new ConcurrentDictionary<string, string>();
        
        elasticClient.SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await sut.SetStateProcessAsync(
            idBpmnProcess,
            tokenProcess,
            nodeStateRegistry,
            errorRegistry,
            dateFromInitInstance);

        // Assert
        await elasticClient.Received(1).SetDataAsync(
            Arg.Is<HistoryNodeState>(state =>
                state.NodeStaus.Length == 3 &&
                state.NodeStaus.Any(n => n.IdNode == "Node1" && n.StatusType == StatusNode.NormalCompleted) &&
                state.NodeStaus.Any(n => n.IdNode == "Node2" && n.StatusType == StatusNode.Works) &&
                state.NodeStaus.Any(n => n.IdNode == "Node3" && n.StatusType == StatusNode.FailedCompleted)));
    }

}