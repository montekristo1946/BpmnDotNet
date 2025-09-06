using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.ElasticClient;
using BpmnDotNet.ElasticClient.Handlers;
using BpmnDotNet.ElasticClient.Tests;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.RollingFileSizeLimit.Extensions;
using Serilog.Sinks.RollingFileSizeLimit.Impl;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        // Можно добавить другие источники конфигурации
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Регистрируем сервисы с разными временами жизни

        services.AddScoped<ElasticClientConfig>();
        services.AddScoped<IElasticClient, ElasticClient>();
        services.AddScoped<IXmlSerializationBpmnDiagramSection, XmlSerializationBpmnDiagramSection>();

        // Регистрируем фоновый сервис
        services.AddHostedService<BackgroundWorkerService>();
    })
    .UseSerilog((ctx, logCfg) =>
    {
        var logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        logCfg
            .MinimumLevel.Is(LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.RollingFileSizeLimited(logDirectoryPath,
                Path.Combine(logDirectoryPath, "Archive"),
                nameof(BpmnDotNet.ElasticClient.Tests),
                fileCompressor: new DefaultFileCompressor(),
                outputTemplate: outputTemplate)
            .WriteTo.Console(outputTemplate: outputTemplate);
    })
    .RunConsoleAsync();