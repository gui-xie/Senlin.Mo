using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Senlin.Mo;

internal static class LogExtensions
{
    public static IServiceCollection ConfigureLog(
        this IServiceCollection services,
        ApplicationLoggerOptions options)
    {
        if (!Enum.TryParse<LogEventLevel>(options.Level, out var logLevel))
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
                    options.Path,
                    retainedFileCountLimit: options.CountLimit,
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information
                )
            ).WriteTo.Async(c => c.Console())
            .CreateLogger();
        return services.AddSerilog();
    }
}