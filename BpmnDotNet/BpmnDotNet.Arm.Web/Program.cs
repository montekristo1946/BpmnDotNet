using BpmnDotNet.Arm.Web.AppWeb;
using BpmnDotNet.Arm.Web.Config;
using BpmnDotNet.Arm.Web.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(SystemConfigure.AppSetting["UseUrls"] ?? string.Empty);
builder.Host.UseLogger();
builder.Host.InitCulture();

builder.Services
        //Add service
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();




app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Log.Information("Service started on url {UseUrls}", SystemConfigure.AppSetting["UseUrls"]);

app.Run();

