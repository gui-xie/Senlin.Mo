using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Senlin.Mo.Middlewares;

public class LogMiddleware(ILogger<LogMiddleware> logger): IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Stopwatch sw = new();
        sw.Start();
        var user = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
            ?.Value ?? "Anonymous";
        await next(context);
        sw.Stop();
        logger.LogInformation("User: {User}, Path: {Path}, Elapsed: {Elapsed}ms", user, context.Request.Path, sw.ElapsedMilliseconds);
    }
}