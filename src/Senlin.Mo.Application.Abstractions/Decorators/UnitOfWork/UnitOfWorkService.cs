namespace Senlin.Mo.Application.Abstractions.Decorators.UnitOfWork;

/// <summary>
/// Unit Of Work Service Attribute
/// </summary>
/// <param name="service"></param>
/// <param name="unitOfWorkHandler"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class UnitOfWorkService<TRequest, TResponse>(
    IService<TRequest, TResponse> service,
    IUnitOfWorkHandler unitOfWorkHandler)
    : IService<TRequest, TResponse>,
        IDecoratorService<UnitOfWorkAttribute>
{
    /// <summary>
    /// Decorator Attribute Data (Injected by DI container)
    /// </summary>
    public UnitOfWorkAttribute AttributeData { get; set; } = null!;

    /// <summary>
    /// Service ExecuteAsync
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        var response = await service.ExecuteAsync(request, cancellationToken);
        if (AttributeData.IsEnable)
        {
            await unitOfWorkHandler.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}