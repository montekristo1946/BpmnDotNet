using BpmnDotNet.Abstractions.Context;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.BPMNDiagram;
using BpmnDotNet.Configuration;
using BpmnDotNet.Dto;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using BpmnDotNet.HistoryDomain.Dto;
using BpmnDotNetIntegrationTests.Context;
using BpmnDotNetIntegrationTests.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog;

namespace BpmnDotNetIntegrationTests.MultiInput;

public class ReceivedTaskMultiInputTest: IDisposable
{
    private readonly IHost _host;
    private readonly IElasticClientSetDataAsync _elasticSetDataAsync;
    private IServiceScope? _scope;
    private readonly IBpmnHandler  _gatewayFirstHandler;
    private readonly IBpmnHandler  _receivedTaskHandler;
    
    public ReceivedTaskMultiInputTest()
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
        
        _receivedTaskHandler = Substitute.For<IBpmnHandler>();
        _receivedTaskHandler.TaskDefinitionId.Returns("ReceivedTaskHandler");
        _receivedTaskHandler.Description.Returns("ReceivedTaskHandler Test description");
        _receivedTaskHandler.ActivityHandlerAsync(Arg.Any<IContextBpmnProcess>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => 
            {
                var context = callInfo.Arg<IContextBpmnProcess>();
                var resGet = context.ReceivedMessage.TryGetValue(typeof(MessageExampleFirst), out var messageExampleFirst);
                if (!resGet || messageExampleFirst is null)
                    throw new OperationCanceledException("Fail try Get key messageExampleFirst");
                
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
                services.AddScoped<IBpmnHandler>(p=>_receivedTaskHandler);
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
            IdBpmnProcess = "ReceivedTaskMultiInputTest",
            TokenProcess = Guid.NewGuid().ToString(),
        };
        contextData.RegistrationMessagesType.TryAdd("ReceivedTaskHandler", typeof(MessageExampleFirst));
        
        var bpmnClient = _host.Services.GetRequiredService<IBpmnClient>();

        
        var taskNode = bpmnClient.StartNewProcess(contextData, TimeSpan.FromSeconds(10));

        bpmnClient.SendMessage(
            contextData.IdBpmnProcess,
            contextData.TokenProcess,
            typeof(MessageExampleFirst),
            new MessageExampleFirst());
      
        await taskNode.ProcessTask;
        
        Assert.Equal(StatusType.Completed,taskNode.StatusType);
    }

  
}