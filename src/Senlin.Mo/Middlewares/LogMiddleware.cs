using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Senlin.Mo.Middlewares;

public class LogMiddleware(ILogger<LogMiddleware> logger): IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var  stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();
        var userId = context.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value 
                     ?? string.Empty;
        var endPoint = context.Request.Path;
        logger.LogInformation(
            "Endpoint: {Endpoint}, UserId: {UserId}, Elapsed: {Elapsed} ms",
            endPoint,
            userId,
            stopwatch.ElapsedMilliseconds);
    }
}