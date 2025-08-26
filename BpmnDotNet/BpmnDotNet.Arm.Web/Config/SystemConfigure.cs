namespace BpmnDotNet.Arm.Web.Config;

internal static class SystemConfigure
{
    static SystemConfigure()
    {
        AppSetting = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Config/SystemConfigure.json")
            .Build();
    }

    public static IConfiguration AppSetting { get; }
}