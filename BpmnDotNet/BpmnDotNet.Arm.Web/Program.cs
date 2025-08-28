using BpmnDotNet.Arm.Core.Abstractions;
using BpmnDotNet.Arm.Core.Handlers;
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
    .AddScoped<IPlanePanelHandler, PlanePanelHandler>()
    .AddScoped<ISvgConstructor, SvgConstructor>()
    .AddScoped<ElasticClientConfig>()
    .AddScoped<IElasticClient, ElasticClient>()
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();


app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Log.Information("Service started on url {UseUrls}", SystemConfigure.AppSetting["UseUrls"]);

app.Run();