using BpmnDotNet.Common;
using BpmnDotNet.Common.Interfases;
using BpmnDotNet.Config;
using BpmnDotNet.Handlers;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.ConsoleApp;
using Sample.ConsoleApp.Context;
using Sample.ConsoleApp.Handlers;
using Serilog;
using Serilog.Events;


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

services.AddBusinessProcess("./BpmnDiagram");

services.AddSingleton<Producer>();
services.AddScoped<SampleService>();


services.AutoRegisterHandlersFromAssemblyOf<ServiceTaskFirstHandler>();


//  Run service
using var provider = services.BuildServiceProvider();

var handlerTypes = provider.GetServices<IBpmnHandler>().ToArray();
var bpmnClient = provider.GetRequiredService<IBpmnClient>();

foreach (var handler in handlerTypes)
{
    //добавить конфигуррирование по времени допустимого выполеннеия
    bpmnClient.RegisterHandlers<IBpmnHandler>(handler);
}


var producer = provider.GetRequiredService<Producer>();
producer.Produce();