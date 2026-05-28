using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Configuration;
using BpmnDotNet.ElasticClientDomain;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Activity;
using BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;

namespace BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain;

public class BpmnClientIntegrationTests
{
    private readonly IElasticClientSetDataAsync _elasticMoq;
    private readonly ServiceCollection _serviceCollection;
    
    public BpmnClientIntegrationTests()
    {
        _elasticMoq = Substitute.For<IElasticClientSetDataAsync>();
        _elasticMoq
            .SetDataAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        _serviceCollection = new ServiceCollection();
        _serviceCollection.AddSingleton<ILoggerFactory>(option =>
        {
            return LoggerFactory.Create(builder => { builder.AddSerilog(Log.Logger); });
        });
        _serviceCollection.AddBusinessProcess("./Handlers/BpmnClientIntegrationDomain/BpmnSource");
        _serviceCollection.AddTransient<ElasticClientConfig>();
        _serviceCollection.AddTransient<IElasticClientSetDataAsync>(_=>_elasticMoq);
        _serviceCollection.AddLogging();
    }
    
    
    [Fact]
    public async Task StartNewProcess_FullPass_FillContext()
    {
        _serviceCollection.AutoRegisterBpmnHandlersFromAssemblyNamespaceOf(typeof(TestActivity));
        await using var provider = _serviceCollection.BuildServiceProvider();

        var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
        var bpmnClient = provider.GetRequiredService<IBpmnClient>();
        bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes);
        
        var contextData = new ContextData
        {
            IdBpmnProcess = "BpmnClientTests",
            TokenProcess = Guid.NewGuid().ToString(),
            TestValue = string.Empty,
        };
        
        var taskNode = bpmnClient.StartNewProcess(contextData, TimeSpan.FromSeconds(5));
        await taskNode.ProcessTask;
        
        Assert.Equal("Competed TestActivity", contextData.TestValue);
    }
    
    [Fact]
    public async Task AutoRegisterHandlersFromAssemblyNamespaceOf_FullPass_FillContext()
    {
        _serviceCollection.AutoRegisterBpmnHandlersFromAssemblyNamespaceOf(typeof(TestActivity));
        
        await using var provider = _serviceCollection.BuildServiceProvider();

        var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
        var bpmnClient = provider.GetRequiredService<IBpmnClient>();
        bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes);
        
        var contextData = new ContextData
        {
            IdBpmnProcess = "BpmnClientTests",
            TokenProcess = Guid.NewGuid().ToString(),
            TestValue = string.Empty,
        };
        
        var taskNode = bpmnClient.StartNewProcess(contextData, TimeSpan.FromSeconds(5));
        await taskNode.ProcessTask;
        
        bpmnClient.Dispose();
        Assert.Equal("Competed TestActivity", contextData.TestValue);
    }
    
    [Fact]
    public async Task AutoRegisterHandlersFromAssemblyOf_FullPass_FillContext()
    {
        _serviceCollection.AutoRegisterBpmnHandlersFromAssemblyOf<TestActivity>();
        
        //  Run service
        await using var provider = _serviceCollection.BuildServiceProvider();

        var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
        var bpmnClient = provider.GetRequiredService<IBpmnClient>();
        bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes);
        
        var contextData = new ContextData
        {
            IdBpmnProcess = "BpmnClientTests",
            TokenProcess = Guid.NewGuid().ToString(),
            TestValue = string.Empty,
        };
        
        var taskNode = bpmnClient.StartNewProcess(contextData, TimeSpan.FromSeconds(5));
        await taskNode.ProcessTask;
        
        bpmnClient.Dispose();
        Assert.Equal("Competed TestActivity", contextData.TestValue);
    }
    
    [Fact]
    public async Task RegisterHandlers_CheckDuplicateRegistration_Exception()
    {
        var testActivity = Substitute.For<IBpmnHandler>();
        testActivity.TaskDefinitionId.Returns("TestActivity");
        testActivity.Description.Returns("Competed TestActivity");
        
        _serviceCollection.AddSingleton<IBpmnHandler>(testActivity);
        
        var testActivity2 = Substitute.For<IBpmnHandler>();
        testActivity2.TaskDefinitionId.Returns("TestActivity");
        testActivity2.Description.Returns("Competed TestActivity");
        
        _serviceCollection.AddSingleton<IBpmnHandler>(testActivity2);
        
        await using var provider = _serviceCollection.BuildServiceProvider();

        var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
        var bpmnClient = provider.GetRequiredService<IBpmnClient>();
        
        
        var exception = Assert.Throws<InvalidOperationException>(() =>
            bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes));
        
        
        Assert.Contains("[BpmnClient:RegisterHandlers] Handler for TaskDefinitionId: TestActivity is already registered", exception.Message);
    }
    
    [Fact]
    public async Task RegisterHandlers_CheckNullTaskDefinitionId_Exception()
    {
        var testActivity = Substitute.For<IBpmnHandler>();
        string taskDefinitionId = null!;
        testActivity.TaskDefinitionId.Returns(taskDefinitionId);
        testActivity.Description.Returns("Competed TestActivity");
        
        _serviceCollection.AddSingleton<IBpmnHandler>(testActivity);
        
        await using var provider = _serviceCollection.BuildServiceProvider();

        var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
        var bpmnClient = provider.GetRequiredService<IBpmnClient>();
        
        var exception = Assert.Throws<InvalidOperationException>(() =>
            bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes));
        
        
        Assert.Contains("TaskDefinitionId is null", exception.Message);
    }
    
    [Fact]
    public async Task RegisterHandlers_CheckNullDescription_Exception()
    {
        var testActivity = Substitute.For<IBpmnHandler>();
        string description = null!;
        testActivity.TaskDefinitionId.Returns("taskDefinitionId");
        testActivity.Description.Returns(description);
        
        _serviceCollection.AddSingleton<IBpmnHandler>(testActivity);
        
        await using var provider = _serviceCollection.BuildServiceProvider();

        var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
        var bpmnClient = provider.GetRequiredService<IBpmnClient>();
        
        var exception = Assert.Throws<InvalidOperationException>(() =>
            bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes));
        
        
        Assert.Contains("Description is null", exception.Message);
    }
    
}