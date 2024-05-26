using Microsoft.EntityFrameworkCore;
using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo.Module;

/// <summary>
/// Unit Of Work
/// </summary>
/// <param name="service"></param>
/// <param name="dbContext"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public class UnitOfWorkService<TRequest, TResponse, TDbContext>(
    IService<TRequest, TResponse> service,
    TDbContext dbContext)
    : IService<TRequest, TResponse>
    where TDbContext : DbContext
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
        await dbContext.SaveChangesAsync(cancellationToken);
        return response;
    }
}