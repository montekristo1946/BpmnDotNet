using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Configuration;
using BpmnDotNet.ElasticClientDomain;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Activity;
using BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BpmnDotNetTests.Handlers.BpmnClientIntegrationDomain;

public class BpmnClientIntegrationTests
{
    private readonly IElasticClientSetDataAsync _elasticMoq;
    
    public BpmnClientIntegrationTests()
    {
        _elasticMoq = Substitute.For<IElasticClientSetDataAsync>();
        _elasticMoq
            .SetDataAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));
    }
    [Fact]
    public async Task StartNewProcess_FullPass_FillContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(_ =>
            LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            }));

        services.AddBusinessProcess("./Handlers/BpmnClientIntegrationDomain/BpmnSource");
        services.AutoRegisterHandlersFromAssemblyOf<TestActivity>();

        services.AddTransient<ElasticClientConfig>();
        services.AddTransient<IElasticClientSetDataAsync>(_=>_elasticMoq);
        services.AddLogging();
        
        //  Run service
        await using var provider = services.BuildServiceProvider();

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
}