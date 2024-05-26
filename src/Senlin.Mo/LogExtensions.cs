using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Senlin.Mo;

internal static class LogExtensions
{
    private const string LogPrefix = "Mo:Log:";

    public static IServiceCollection ConfigureLog(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var countLimit = configuration.GetValue<int>($"{LogPrefix}CountLimit");
        var path = configuration.GetValue<string>($"{LogPrefix}Path")!;
        var level = configuration.GetValue<string>($"{LogPrefix}Level");
        if (!Enum.TryParse<LogEventLevel>(level, out var logLevel))
        {
            logLevel = LogEventLevel.Debug;
        }

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c =>
                c.File(
                    path,
                    retainedFileCountLimit: countLimit,
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information
                )
            ).WriteTo.Async(c => c.Console())
            .CreateLogger();
        return services.AddSerilog();
    }
}