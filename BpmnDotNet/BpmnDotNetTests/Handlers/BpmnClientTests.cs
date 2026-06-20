using AutoFixture;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Dto;
using BpmnDotNet.Dto;
using BpmnDotNet.Handlers;
using BpmnDotNet.HistoryDomain.Abstractions;
using BpmnDotNetTests.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class BpmnClientTests
{
    private readonly BpmnProcessDto[] _businessProcessDtos;
    private readonly ILoggerFactory _loggerFactory;
    private readonly   IHistoryNodeStateWriter _historyNodeStateWriter;
    private readonly IDescriptionWriteService _descriptionWriteService;
    private BpmnClient _bpmnClient;
    private readonly IFixture _fixture;
    private readonly IBpmnEngine  _bpmnEngine;
    private readonly IProcessModelBuilder _processModelBuilder;
  
    
    public BpmnClientTests()
    {
        _businessProcessDtos = new BpmnProcessDto[]{};
        _loggerFactory  = Substitute.For<ILoggerFactory>();
        _bpmnEngine = Substitute.For<IBpmnEngine>();
        _historyNodeStateWriter = Substitute.For<IHistoryNodeStateWriter>();
        _descriptionWriteService = Substitute.For<IDescriptionWriteService>();
        _processModelBuilder= Substitute.For<IProcessModelBuilder>();
        _bpmnClient = new BpmnClient(
            _businessProcessDtos,
            _loggerFactory,
            _historyNodeStateWriter,
            _descriptionWriteService,
            _processModelBuilder,
            TimeSpan.FromMilliseconds(1000));
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ClearBpmnProcessesDictionary_FullPassNormalFinalizedProcess_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
       var resAdd = _bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

       await _bpmnClient.ClearBpmnProcessesDictionaryAsync();
       
       Assert.True(resAdd);
       Assert.Empty(_bpmnClient.BpmnProcesses.Keys);
       
       await _bpmnClient.DisposeAsync();
    }
    
    [Fact]
    public async Task ClearBpmnProcessesDictionary_NoNormalCompletedFinalizedProcess_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
        var resAdd = _bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        await _bpmnClient.ClearBpmnProcessesDictionaryAsync();
       
        Assert.True(resAdd);
        Assert.Empty(_bpmnClient.BpmnProcesses.Keys);
       
        await _bpmnClient.DisposeAsync();
    }
    
    [Fact]
    public async Task ClearBpmnProcessesDictionary_CheckForceCall_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
        var resAdd = _bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        await _bpmnClient.ClearBpmnProcessesDictionaryAsync(true);
       
        Assert.True(resAdd);
        Assert.Empty(_bpmnClient.BpmnProcesses.Keys);
       
        await _bpmnClient.DisposeAsync();
    }
    
    [Fact]
    public async Task ClearBpmnProcessesDictionary_CheckWaitPending_CountProcessInBpmnProcesses()
    {
        var idBpmnProcess = Guid.NewGuid().ToString();
        var tokenProcess = Guid.NewGuid().ToString();
        var newBusinessProcess = new BusinessProcessJobStatus()
        {
            IdBpmnProcess = idBpmnProcess,
            TokenProcess = tokenProcess
        };
        
        var resAdd = _bpmnClient.BpmnProcesses.TryAdd((idBpmnProcess, tokenProcess), newBusinessProcess);

        await _bpmnClient.ClearBpmnProcessesDictionaryAsync();
       
        Assert.True(resAdd);
        Assert.Single(_bpmnClient.BpmnProcesses.Keys);
       
        await _bpmnClient.DisposeAsync();
    }
    
    [Fact]
    public void SendMessage_ShouldAddMessageToProcess_WhenProcessExists()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        var mockProcess = Substitute.For<IBpmnEngine>();
        mockProcess.AddMessageToQueue(messageType, message).Returns(true);
        
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, mockProcess, _bpmnClient);
        
        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message);
        
        // Assert
        mockProcess.Received(1).AddMessageToQueue(messageType, message);
    }
    
    [Fact]
    public void SendMessage_ShouldWorkWithDifferentMessageTypes()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(int);
        var message = _fixture.Create<int>();
        
        _bpmnEngine.AddMessageToQueue(Arg.Any<Type>(), Arg.Any<object>()).Returns(true);
        
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine, _bpmnClient);
        
        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message);
        
        // Assert
        _bpmnEngine.Received(1).AddMessageToQueue(messageType, message);
    }
    
    [Fact]
    public void SendMessage_ShouldCallAddMessageToQueue_WithCorrectParameters()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(decimal);
        var message = _fixture.Create<decimal>();
        
     
        _bpmnEngine.AddMessageToQueue(Arg.Any<Type>(), Arg.Any<object>()).Returns(true);
        
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine, _bpmnClient);
        
        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message);
        
        // Assert
        _bpmnEngine.Received(1).AddMessageToQueue(
            Arg.Is<Type>(t => t == messageType),
            Arg.Is<object>(o => o.Equals(message)));
    }
    
    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenProcessNotFound()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Не добавляем процесс в хранилище
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));
        
        Assert.Contains($"[SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenProcessIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Добавляем процесс с null значением Process
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, null, _bpmnClient);
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));
        
        Assert.Contains($"[SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenProcessJobStatusIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Добавляем null значение в словарь
        var processes = BpmnClientTestHelper.GetBpmnProcessesDictionary(_bpmnClient);
        processes[(idBpmnProcess, tokenProcess)] = null!;
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));
        
        Assert.Contains($"[SendMessage] Not find bpmnProcesses: {idBpmnProcess} {tokenProcess}", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenAddMessageToQueueReturnsFalse()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        _bpmnEngine.AddMessageToQueue(Arg.Any<Type>(), Arg.Any<object>()).Returns(false);
        
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine,_bpmnClient);
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));
        
        Assert.Contains($"[SendMessage] Not Add message : {idBpmnProcess} {tokenProcess} {messageType}", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowInvalidOperationException_WhenAddMessageToQueueThrowsException()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
  
        _bpmnEngine.When(x => x.AddMessageToQueue(messageType, message))
            .Do(x => throw new InvalidOperationException("Test error from process"));
        
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess, _bpmnEngine,_bpmnClient);
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, message));
        
        Assert.Contains($"Test error from process", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldHandleDifferentProcesses_WithSameIdButDifferentTokens()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess1 = _fixture.Create<string>();
        var tokenProcess2 = _fixture.Create<string>();
        var messageType = typeof(string);
        var message1 = _fixture.Create<string>();
        var message2 = _fixture.Create<string>();
        
        var mockProcess1 = Substitute.For<IBpmnEngine>();
        var mockProcess2 = Substitute.For<IBpmnEngine>();
        
        mockProcess1.AddMessageToQueue(messageType, message1).Returns(true);
        mockProcess2.AddMessageToQueue(messageType, message2).Returns(true);
        
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess1, mockProcess1,_bpmnClient);
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess, tokenProcess2, mockProcess2,_bpmnClient);
        
        // Act
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess1, messageType, message1);
        _bpmnClient.SendMessage(idBpmnProcess, tokenProcess2, messageType, message2);
        
        // Assert
        mockProcess1.Received(1).AddMessageToQueue(messageType, message1);
        mockProcess2.Received(1).AddMessageToQueue(messageType, message2);
    }
    
    [Fact]
    public void SendMessage_ShouldHandleDifferentProcesses_WithDifferentIdsAndSameTokens()
    {
        // Arrange
        var idBpmnProcess1 = _fixture.Create<string>();
        var idBpmnProcess2 = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message1 = _fixture.Create<string>();
        var message2 = _fixture.Create<string>();
        
        var mockProcess1 = Substitute.For<IBpmnEngine>();
        var mockProcess2 = Substitute.For<IBpmnEngine>();
        
        mockProcess1.AddMessageToQueue(messageType, message1).Returns(true);
        mockProcess2.AddMessageToQueue(messageType, message2).Returns(true);
        
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess1, tokenProcess, mockProcess1,_bpmnClient);
        BpmnClientTestHelper.AddProcessToStore(idBpmnProcess2, tokenProcess, mockProcess2,_bpmnClient);
        
        // Act
        _bpmnClient.SendMessage(idBpmnProcess1, tokenProcess, messageType, message1);
        _bpmnClient.SendMessage(idBpmnProcess2, tokenProcess, messageType, message2);
        
        // Assert
        mockProcess1.Received(1).AddMessageToQueue(messageType, message1);
        mockProcess2.Received(1).AddMessageToQueue(messageType, message2);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenIdBpmnProcessIsNull()
    {
        // Arrange
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(null!, tokenProcess, messageType, message));
        
        Assert.Contains("idBpmnProcess", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenIdBpmnProcessIsEmpty()
    {
        // Arrange
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage(string.Empty, tokenProcess, messageType, message));
        
        Assert.Contains("idBpmnProcess", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenIdBpmnProcessIsWhiteSpace()
    {
        // Arrange
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage("   ", tokenProcess, messageType, message));
        
        Assert.Contains("idBpmnProcess", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenTokenProcessIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, null!, messageType, message));
        
        Assert.Contains("tokenProcess", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenTokenProcessIsEmpty()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, string.Empty, messageType, message));
        
        Assert.Contains("tokenProcess", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentException_WhenTokenProcessIsWhiteSpace()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, "   ", messageType, message));
        
        Assert.Contains("tokenProcess", exception.Message);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentNullException_WhenMessageTypeIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var message = _fixture.Create<string>();
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, null!, message));
        
        Assert.Equal("messageType", exception.ParamName);
    }
    
    [Fact]
    public void SendMessage_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Arrange
        var idBpmnProcess = _fixture.Create<string>();
        var tokenProcess = _fixture.Create<string>();
        var messageType = typeof(string);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            _bpmnClient.SendMessage(idBpmnProcess, tokenProcess, messageType, null!));
        
        Assert.Equal("message", exception.ParamName);
    }

}