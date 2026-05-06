using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.SvgDomain.Service;
using BpmnDotNet.Arm.Core.UiDomain.Abstractions;
using BpmnDotNet.Arm.Core.UiDomain.Services;
using BpmnDotNet.Arm.Web.AppWeb;
using BpmnDotNet.Arm.Web.Extensions;
using BpmnDotNet.ElasticClientDomain;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddCommandLine(args);  // аргументы командной строки имеют приоритет

var useUrls = builder.Configuration["UseUrls"] ?? "http://localhost:5002";

builder.WebHost.UseUrls(useUrls);
builder.Host.UseLogger();
builder.Host.InitCulture();

builder.Services
    .AddScoped<ISvgConstructor, SvgConstructor>()
    .AddScoped<ElasticClientConfig>(serviceProvider => new ElasticClientConfig()
    {
        ConnectionString = builder.Configuration["ElasticConnectionUrl"] ?? throw new NullReferenceException("ElasticConnectionUrl empty"),
    })
    .AddScoped<IElasticClient, ElasticClient>()
    .AddTransient<IPlanePanelHandler, PlanePanelHandler>()
    .AddTransient<IFilterPanelHandler, FilterPanelHandler>()
    .AddTransient<IListProcessPanelHandler, ListProcessPanelHandler>()
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Log.Information("Service started on url {UseUrls}", useUrls);

app.Run();