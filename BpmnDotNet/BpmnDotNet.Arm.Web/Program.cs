using BpmnDotNet.Arm.Core.SvgDomain.Abstractions;
using BpmnDotNet.Arm.Core.SvgDomain.Service;
using BpmnDotNet.Arm.Core.UiDomain.Abstractions;
using BpmnDotNet.Arm.Core.UiDomain.Services;
using BpmnDotNet.Arm.Web.AppWeb;
using BpmnDotNet.Arm.Web.Config;
using BpmnDotNet.Arm.Web.Extensions;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.ElasticClient;
using BpmnDotNet.ElasticClient.Handlers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(SystemConfigure.AppSetting["UseUrls"] ?? string.Empty);
builder.Host.UseLogger();
builder.Host.InitCulture();

builder.Services
    .AddScoped<ISvgConstructor, SvgConstructor>()
    .AddScoped<ElasticClientConfig>(serviceProvider => new ElasticClientConfig()
    {
        ConnectionString = SystemConfigure.AppSetting["ElasticConnectionUrl"] ?? throw new NullReferenceException("ElasticConnectionUrl empty"),
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

Log.Information("Service started on url {UseUrls}", SystemConfigure.AppSetting["UseUrls"]);

app.Run();