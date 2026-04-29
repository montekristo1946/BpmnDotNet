using BpmnDotNet.Abstractions.Common;
using BpmnDotNet.Config;
using BpmnDotNet.ElasticClientDomain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp;
using Sample.ConsoleApp.Handlers;
using Sample.ConsoleApp.Service;
using Serilog;

//Логгер.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("./Logs/Sample.ConsoleApp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var host = Host.CreateDefaultBuilder()
    .UseDefaultServiceProvider((context, options) =>
    {
        options.ValidateScopes       = true;   // проверка lifetimes
        options.ValidateOnBuild      = true;   // проверка при Build()
    })
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
        services.AddSingleton<SampleService>();
        services.AddLogging();
        
        //Регистрация IBpmnHandler в IBpmnClient.
        //Необходимо делать после инициализации IBpmnClient,
        //чтоб избежать циклических зависимостей в компонентах.
        services.AddHostedService<BpmnClientHost>();
        
    })
    .Build();
    
await host.StartAsync();
var producer = host.Services.GetRequiredService<Producer>();

producer.Produce();

await host.StopAsync();
host.Dispose();