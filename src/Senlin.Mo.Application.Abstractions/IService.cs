using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Domain;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Service interface
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IService<in TRequest, TResponse>
{
    /// <summary>
    /// Execute service
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken );
}