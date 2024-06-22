using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Senlin.Mo.Application.Abstractions.Decorators;

/// <summary>
/// Log Service
/// </summary>
/// <param name="service"></param>
/// <param name="logger"></param>
/// <param name="getUserId"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class LogServiceAttribute<TRequest, TResponse>(
    IService<TRequest, TResponse> service,
    ILogger<LogServiceAttribute<TRequest, TResponse>> logger,
    GetUserId getUserId) : IService<TRequest, TResponse>
{
    /// <summary>
    /// Is enable
    /// </summary>
    public bool IsEnable { get; set; } = true;

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
        Stopwatch? stopwatch = null;
        if (IsEnable)
        {
            stopwatch = Stopwatch.StartNew();
        }

        var response = await service.ExecuteAsync(request, cancellationToken);
        if (!IsEnable) return response;

        stopwatch!.Stop();
        logger.LogInformation(
            "Request: {Request}, UserId: {UserId}, Elapsed: {Elapsed} ms",
            request?.GetType().Name,
            getUserId(),
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}