using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Log Service
/// </summary>
/// <param name="service"></param>
/// <param name="logger"></param>
/// <param name="getUserId"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class LogDecorator<TRequest, TResponse>(
    IService<TRequest, TResponse> service,
    ILogger logger,
    GetUserId getUserId): IService<TRequest, TResponse>
{
    /// <summary>
    /// Service ExecuteAsync
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await service.ExecuteAsync(request, cancellationToken);
        stopwatch.Stop();
        logger.LogInformation(
            "Request: {Request}, UserId: {UserId}, Elapsed: {Elapsed} ms",
            request?.GetType().Name,
            getUserId(),
            stopwatch.ElapsedMilliseconds);
        return response;
    }
}