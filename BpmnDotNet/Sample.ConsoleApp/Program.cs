using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Config;
using BpmnDotNet.ElasticClient;
using BpmnDotNet.ElasticClient.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp;
using Sample.ConsoleApp.Handlers;
using Sample.ConsoleApp.Moq;
using Serilog;

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
services.AddTransient<IElasticClientSetDataAsync, ElasticClientMoq>();

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

var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
var bpmnClient = provider.GetRequiredService<IBpmnClient>();

//Регистрация IBpmnHandler в IBpmnClient.
bpmnClient.RegisterHandlers<IBpmnHandler>(handlerTypes);


var producer = provider.GetRequiredService<Producer>();
producer.Produce();