namespace Senlin.Mo.Application.Abstractions.Decorators;

/// <summary>
/// Unit Of Work Service Attribute
/// </summary>
/// <param name="service"></param>
/// <param name="unitOfWorkHandler"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class UnitOfWorkServiceAttribute<TRequest, TResponse>(
    IService<TRequest, TResponse> service,
    IUnitOfWorkHandler unitOfWorkHandler)
    : Attribute, IService<TRequest, TResponse>
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
    public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
    {
        var response = await service.ExecuteAsync(request, cancellationToken);
        if (IsEnable)
        {
            await unitOfWorkHandler.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}