namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Unit Of Work
/// </summary>
/// <param name="service"></param>
/// <param name="unifOfWorkHandler"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public class UnitOfWorkDecorator<TRequest, TResponse, TDbContext>(
    IService<TRequest, TResponse> service,
    IUnitOfWorkHandler unifOfWorkHandler)
    : IService<TRequest, TResponse>
{
    /// <summary>
    /// Service ExecuteAsync
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        var response = await service.ExecuteAsync(request, cancellationToken);
        await unifOfWorkHandler.SaveChangesAsync(cancellationToken);
        return response;
    }
}

public interface IUnitOfWorkHandler
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}