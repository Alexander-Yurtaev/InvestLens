using Microsoft.AspNetCore.Builder;
using Serilog;

namespace InvestLens.Shared.Helpers;

public static class SerilogHelper
{
    public static ILogger CreateLogger(WebApplicationBuilder builder)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration.GetSection("Serilog"))
            .Enrich.WithProperty("ServiceName", builder.Environment.ApplicationName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        return logger;
    }
}