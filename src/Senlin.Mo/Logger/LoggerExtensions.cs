using Microsoft.Extensions.Logging;
using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo;

internal static class LoggerExtensions
{
    public static Type GetLoggerType(this IModule module)
    {
        return typeof(ILogger<>).MakeGenericType(module.GetType());
    }
}