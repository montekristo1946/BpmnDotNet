using System.Collections.Concurrent;
using System.Reflection;
using AutoFixture;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Elements;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers;

public class BusinessProcessTest:IDisposable
{
    private readonly IFixture _fixture;
    private readonly IContextBpmnProcess _contextBpmnProcess;
    private readonly ILogger<IBusinessProcess> _logger;
    private readonly BpmnProcessDto _bpmnSchema;
    private readonly IPathFinder _pathFinder;
    private readonly ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>> _handlers;
    private readonly TimeSpan _timeout;
    private readonly IHistoryNodeStateWriter _historyNodeStateWriter;
    private readonly BusinessProcess _businessProcess;
    
    public BusinessProcessTest()
    {
        _fixture = new Fixture();
        
        // Создаем моки для всех зависимостей
        _contextBpmnProcess = Substitute.For<IContextBpmnProcess>();
        _logger = Substitute.For<ILogger<IBusinessProcess>>();
        _bpmnSchema = new BpmnProcessDto(string.Empty,[]);
        _pathFinder = Substitute.For<IPathFinder>();
        _handlers = new ConcurrentDictionary<string, Func<IContextBpmnProcess, CancellationToken, Task>>();
        _timeout = TimeSpan.FromSeconds(30);
        _historyNodeStateWriter = Substitute.For<IHistoryNodeStateWriter>();
        
        // Настройка необходимых свойств для contextBpmnProcess
        _contextBpmnProcess.IdBpmnProcess.Returns(_fixture.Create<string>());
        _contextBpmnProcess.TokenProcess.Returns(_fixture.Create<string>());
        
        // Создаем экземпляр BusinessProcess
        _businessProcess = new BusinessProcess(
            _contextBpmnProcess,
            _logger,
            _bpmnSchema,
            _pathFinder,
            _handlers,
            _timeout,
            _historyNodeStateWriter);
    }

    public void Dispose()
    {
        _businessProcess.Dispose();
    }
    
    private static ConcurrentDictionary<Type, object> GetMessagesStore(BusinessProcess businessProcess)
    {
        var field = typeof(BusinessProcess)
            .GetField("_messagesStore", BindingFlags.NonPublic | BindingFlags.Instance);
        
        return field?.GetValue(businessProcess) as ConcurrentDictionary<Type, object> 
               ?? throw new InvalidOperationException("Cannot get _messagesStore field");
    }
    
    [Fact]
    public void AddMessageToQueue_ShouldStoreMessageInMessagesStore()
    {
        // Arrange
        var messageType = typeof(string);
        var message = _fixture.Create<string>();
        
        // Act
        var result = _businessProcess.AddMessageToQueue(messageType, message);
        
        // Assert
        Assert.True(result);
        
        var messagesStore = GetMessagesStore(_businessProcess);
        Assert.NotNull(messagesStore);
        Assert.Contains(messageType, messagesStore.Keys);
        Assert.Equal(message, messagesStore[messageType]);
    }

    [Fact]
    public void AddMessageToQueue_ShouldThrowArgumentNullException_WhenMessageTypeIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            _businessProcess.AddMessageToQueue(null!, _fixture.Create<string>()));
        
        Assert.Equal("messageType", exception.ParamName);
        
        // Проверяем, что сообщение не было добавлено
        var messagesStore = GetMessagesStore(_businessProcess);
        Assert.Empty(messagesStore);
    }
    
    [Fact]
    public void AddMessageToQueue_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Arrange
        var messageType = typeof(string);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            _businessProcess.AddMessageToQueue(messageType, null!));
        
        Assert.Equal("message", exception.ParamName);
        
        // Проверяем, что сообщение не было добавлено
        var messagesStore = GetMessagesStore(_businessProcess);
        Assert.Empty(messagesStore);
    }
    
    [Fact]
    public void AddMessageToQueue_ShouldReplaceExistingMessage_WhenSameTypeAddedAgain()
    {
        // Arrange
        var messageType = typeof(int);
        var oldMessage = _fixture.Create<int>();
        var newMessage = _fixture.Create<int>();
        
        // Act
        _businessProcess.AddMessageToQueue(messageType, oldMessage);
        _businessProcess.AddMessageToQueue(messageType, newMessage);
        
        // Assert
        var messagesStore = GetMessagesStore(_businessProcess);
        Assert.NotNull(messagesStore);
        Assert.Equal(newMessage, messagesStore[messageType]);
        Assert.Single(messagesStore);
        
        // Проверяем, что логгер не вызывался
        _logger.DidNotReceive().Log(
            Arg.Is<LogLevel>(x => x == LogLevel.Error),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
    
    [Fact]
    public void AddMessageToQueue_ShouldHandleMultipleMessageTypes()
    {
        // Arrange
        var message1Type = typeof(string);
        var message1 = _fixture.Create<string>();
        
        var message2Type = typeof(int);
        var message2 = _fixture.Create<int>();
        
        var message3Type = typeof(double);
        var message3 = _fixture.Create<double>();
        
        var message4Type = typeof(Guid);
        var message4 = _fixture.Create<Guid>();
        
        // Act
        _businessProcess.AddMessageToQueue(message1Type, message1);
        _businessProcess.AddMessageToQueue(message2Type, message2);
        _businessProcess.AddMessageToQueue(message3Type, message3);
        _businessProcess.AddMessageToQueue(message4Type, message4);
        
        // Assert
        var messagesStore = GetMessagesStore(_businessProcess);
        Assert.NotNull(messagesStore);
        Assert.Equal(4, messagesStore.Count);
        Assert.Equal(message1, messagesStore[message1Type]);
        Assert.Equal(message2, messagesStore[message2Type]);
        Assert.Equal(message3, messagesStore[message3Type]);
        Assert.Equal(message4, messagesStore[message4Type]);
    }
    
    [Theory]
    [InlineData(typeof(string), "test")]
    [InlineData(typeof(int), 42)]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(Guid), "00000000-0000-0000-0000-000000000000")]
    [InlineData(typeof(double), 3.14)]
    [InlineData(typeof(decimal), 99.99)]
    [InlineData(typeof(DateTime), "2024-01-01")]
    public void AddMessageToQueue_ShouldStoreVariousMessageTypes(Type messageType, object message)
    {
        // Act
        var result = _businessProcess.AddMessageToQueue(messageType, message);
        
        // Assert
        Assert.True(result);
        
        var messagesStore = GetMessagesStore(_businessProcess);
        Assert.NotNull(messagesStore);
        Assert.Contains(messageType, messagesStore.Keys);
        Assert.Equal(message, messagesStore[messageType]);
    }

}