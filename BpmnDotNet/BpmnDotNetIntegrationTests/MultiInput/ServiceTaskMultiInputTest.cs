using AutoFixture;
using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.Configuration;
using BpmnDotNet.Dto;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using BpmnDotNetIntegrationTests.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;

namespace BpmnDotNetIntegrationTests;

public class ServiceTaskMultiInputTest: IDisposable
{
    private readonly IHost _host;
    private readonly IElasticClientSetDataAsync _elasticSetDataAsync;
    private IServiceScope? _scope;
    private readonly IBpmnHandler  _gatewayFirstHandler;
    
    public ServiceTaskMultiInputTest()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        
        _elasticSetDataAsync = Substitute.For<IElasticClientSetDataAsync>();
        _elasticSetDataAsync.SetDataAsync(Arg.Any<BpmnPlane>()).Returns(Task.FromResult(true));
        _elasticSetDataAsync.SetDataAsync(Arg.Any<HistoryNodeState>()).Returns(Task.FromResult(true));
        _elasticSetDataAsync.SetDataAsync(Arg.Any<DescriptionData>(),Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        _gatewayFirstHandler = Substitute.For<IBpmnHandler>();
        _gatewayFirstHandler.TaskDefinitionId.Returns("GatewayFirstHandler");
        _gatewayFirstHandler.Description.Returns("GatewayFirstHandler Test description");
        _gatewayFirstHandler.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => 
            {
                var context = callInfo.Arg<IContextBpmnProcess>();
                var conditionRoute = context as IExclusiveGateWayRoute;

                if (conditionRoute is null)
                    throw new OperationCanceledException("Fail try Add key ConditionRoute");

                conditionRoute.ConditionRoute.TryAdd("GatewayFirstHandler", "Flow_in_SendTaskFirstHandler");
                
                return Task.CompletedTask;
            });

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ILoggerFactory>(option =>
                {
                    return LoggerFactory.Create(builder => { builder.AddSerilog(Log.Logger); });
                });

                services.AddSingleton<IElasticClientSetDataAsync>(_elasticSetDataAsync);
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BpmnDiagram");
                services.AddBusinessProcess(path);
                services.AddScoped<IBpmnHandler>(p=>_gatewayFirstHandler);
            })
            .UseSerilog()
            .Build();
        
        _scope =_host.Services.CreateScope();
        
        var handlerTypes = _scope.ServiceProvider.GetServices<IBpmnHandler>()?.ToArray() ?? [];
        if (!handlerTypes.Any())
        {
            throw new InvalidOperationException("[BpmnClientHost] Bpmn activity not found");
        }

        var bpmnClient = _host.Services.GetRequiredService<IBpmnClient>();
        bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes);
    }
    
    public void Dispose()
    {
        _scope?.Dispose();
    }
    
    [Fact]
    public async Task StartNewProcess_CheckServiceTaskMultiInput_Completed()
    {
        var contextData = new ContextData()
        {
            IdBpmnProcess = "ServiceTaskMultiInputTest",
            TokenProcess = Guid.NewGuid().ToString(),
        };
        
        var bpmnClient = _host.Services.GetRequiredService<IBpmnClient>();
        var taskNode = bpmnClient.StartNewProcess(contextData, TimeSpan.FromSeconds(10));

        await taskNode.ProcessTask;
        
        Assert.Equal(StatusType.Completed,taskNode.StatusType);
    }

  
}