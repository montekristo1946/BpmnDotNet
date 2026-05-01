using AutoFixture;
using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Dto;
using BpmnDotNet.Handlers;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class HistoryNodeStateWriterTest
{
    private readonly IFixture _fixture;
    private readonly IElasticClientSetDataAsync _elasticClient;
    private readonly ILogger<HistoryNodeStateWriter> _logger;
    private readonly HistoryNodeStateWriter _sut;

    public HistoryNodeStateWriterTest()
    {
        _fixture = new Fixture();
        
        _fixture.Customize<NodeJobStatus>(c => c
            .With(x => x.IdNode, Guid.NewGuid().ToString())
            .With(x => x.StatusType, StatusType.Works));
        
        _elasticClient = Substitute.For<IElasticClientSetDataAsync>();
        _logger = Substitute.For<ILogger<HistoryNodeStateWriter>>();
        _sut = new HistoryNodeStateWriter(_elasticClient, _logger);
    }
    
    [Fact]
    public async Task SetStateProcessAsync_WhenAllParametersValid_ShouldCallSetDataAsync()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var nodeStateRegistry = _fixture.CreateMany<NodeJobStatus>(3).ToArray();
        var arrayMessageErrors = _fixture.CreateMany<string>(2).ToArray();
        var dateFromInitInstance = _fixture.Create<long>();
        
        _elasticClient
            .SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(true);

        // Act
        await _sut.SetStateProcessAsync(
            idBpmnProcess, 
            tokenProcess, 
            nodeStateRegistry, 
            arrayMessageErrors, 
            false, 
            dateFromInitInstance);

        // Assert
        await _elasticClient
            .Received(1)
            .SetDataAsync(Arg.Is<HistoryNodeState>(state =>
                state.IdBpmnProcess == idBpmnProcess &&
                state.TokenProcess == tokenProcess &&
                state.NodeStaus.Length == 3 &&
                state.ArrayMessageErrors.Length == 2 &&
                state.DateCreated == dateFromInitInstance &&
                state.DateLastModified > 0));
    }
    
    [Fact]
    public async Task SetStateProcessAsync_ShouldMapNodeJobStatusCorrectly()
    {
        // Arrange
        var nodeStateRegistry = _fixture
            .Build<NodeJobStatus>()
            .With(x => x.IdNode, "node-1")
            .With(x => x.StatusType, StatusType.Works)
            .CreateMany(2)
            .ToArray();

        HistoryNodeState? capturedState = null;
        _elasticClient
            .SetDataAsync(Arg.Do<HistoryNodeState>(state => capturedState = state))
            .Returns(true);

        // Act
        await _sut.SetStateProcessAsync(
            "process-1", 
            "token-1", 
            nodeStateRegistry, 
            Array.Empty<string>(), 
            false, 
            100);

        // Assert
        Assert.NotNull(capturedState);
        Assert.Equal(2, capturedState!.NodeStaus.Length);
        Assert.Equal("node-1", capturedState.NodeStaus[0].IdNode);
        Assert.Equal(StatusType.Works, capturedState.NodeStaus[0].StatusType);
    }
    
    [Fact]
    public async Task SetStateProcessAsync_WhenSetDataReturnsFalse_ShouldLogError()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        
        _elasticClient
            .SetDataAsync(Arg.Any<HistoryNodeState>())
            .Returns(false);

        // Act
        await _sut.SetStateProcessAsync(
            idBpmnProcess, 
            tokenProcess, 
            Array.Empty<NodeJobStatus>(), 
            Array.Empty<string>(), 
            false, 
            0);

        // Assert
        _logger
            .Received(1)
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains(idBpmnProcess) && 
                                    o.ToString()!.Contains(tokenProcess)),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Theory]
    [InlineData(null, "token")]
    [InlineData("", "token")]
    [InlineData(" ", "token")]
    public async Task SetStateProcessAsync_WhenIdBpmnProcessInvalid_ShouldThrow(
        string? idBpmnProcess, string tokenProcess)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.SetStateProcessAsync(
                idBpmnProcess!,
                tokenProcess,
                Array.Empty<NodeJobStatus>(),
                Array.Empty<string>(),
                false,
                0));
    }
    
    [Theory]
    [InlineData("id", null)]
    [InlineData("id", "")]
    [InlineData("id", " ")]
    public async Task SetStateProcessAsync_WhenTokenProcessInvalid_ShouldThrow(
        string idBpmnProcess, string? tokenProcess)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sut.SetStateProcessAsync(
                idBpmnProcess,
                tokenProcess,
                Array.Empty<NodeJobStatus>(),
                Array.Empty<string>(),
                false,
                0));
    }
    
    [Theory]
    [MemberData(nameof(HistoryNodeStateWriterTestHelper.GetNodeStateScenarios), 
        MemberType = typeof(HistoryNodeStateWriterTestHelper))]
    public async Task SetStateProcessAsync_ShouldCalculateStatusCorrectly(
        NodeJobStatus[] nodeStates, 
        bool isCompleted, 
        ProcessStatus expectedStatus)
    {
        // Arrange
        ProcessStatus capturedStatus = ProcessStatus.None;
        _elasticClient
            .SetDataAsync(Arg.Do<HistoryNodeState>(state => 
                capturedStatus = state.ProcessStatus))
            .Returns(true);

        // Act
        await _sut.SetStateProcessAsync(
            "process-1", 
            "token-1", 
            nodeStates, 
            Array.Empty<string>(), 
            isCompleted, 
            0);

        // Assert
        Assert.Equal(expectedStatus, capturedStatus);
    }

}