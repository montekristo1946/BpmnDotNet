using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Config;
using BpmnDotNet.ElasticClientDomain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp;
using Sample.ConsoleApp.Handlers;
using Sample.ConsoleApp.Service;
using Serilog;

/*
//Логер
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("./Logs/Sample.ConsoleApp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


//  Service registration pipeline...
var services = new ServiceCollection();
services.AddSingleton<ILoggerFactory>(option =>
{
    return LoggerFactory.Create(builder => { builder.AddSerilog(Log.Logger); });
});

//Инструменты для записи в ElasticSearch
services.AddTransient<ElasticClientConfig>();
services.AddTransient<IElasticClientSetDataAsync, ElasticClient>();

//Создадим IBpmnClient, загрузим схемы
services.AddBusinessProcess("./BpmnDiagram");

//Зарегистрирует все Handlers реализующие интерфейс IBpmnHandler
services.AutoRegisterHandlersFromAssemblyOf<ServiceTaskFirstHandler>();

//Вспомогательные классы для тестовой консоли.
services.AddSingleton<Producer>();
services.AddScoped<SampleService>();
services.AddLogging();

// Run service
using var provider = services.BuildServiceProvider();

// var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
// var bpmnClient = provider.GetRequiredService<IBpmnClient>();
//
// //Регистрация IBpmnHandler в IBpmnClient.
// bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes);

// services.AddHostedService<BpmnClientHost>();

var producer = provider.GetRequiredService<Producer>();
producer.Produce();*/

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("./Logs/Sample.ConsoleApp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ILoggerFactory>(option =>
        {
            return LoggerFactory.Create(builder => { builder.AddSerilog(Log.Logger); });
        });
        
        //Инструменты для записи в ElasticSearch
        services.AddTransient<ElasticClientConfig>();
        services.AddTransient<IElasticClientSetDataAsync, ElasticClient>();

        //Создадим IBpmnClient, загрузим схемы
        services.AddBusinessProcess("./BpmnDiagram");

        //Зарегистрирует все Handlers реализующие интерфейс IBpmnHandler
        services.AutoRegisterHandlersFromAssemblyOf<ServiceTaskFirstHandler>();

        //Вспомогательные классы для тестовой консоли.
        services.AddSingleton<Producer>();
        services.AddScoped<SampleService>();
        services.AddLogging();
        
        services.AddHostedService<BpmnClientHost>();
        
    })
    .Build();
    
await host.StartAsync();

var producer = host.Services.GetRequiredService<Producer>();
producer.Produce();

await host.StopAsync();
host.Dispose();