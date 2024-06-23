namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Service interface
/// </summary>
public interface IService
{
}

/// <summary>
/// Service interface
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IService<in TRequest, TResponse> : IService
{
    /// <summary>
    /// Execute service
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken);
}