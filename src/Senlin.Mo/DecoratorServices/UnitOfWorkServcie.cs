using Microsoft.Extensions.Logging;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Application.Abstractions.Decorators;
using Senlin.Mo.Domain;

namespace Senlin.Mo.DecoratorServices;

/// <summary>
/// Unit Of Work Service Attribute
/// </summary>
/// <param name="service"></param>
/// <param name="unitOfWorkHandler"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class UnitOfWorkService<TRequest, TResponse>(
    IService<TRequest, TResponse> service,
    IUnitOfWorkHandler unitOfWorkHandler,
    IEventExecutor eventExecutor,
    ILogger<UnitOfWorkService<TRequest, TResponse>> logger)
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
        if (!AttributeData.IsEnable) return response;
        if (response is not IResult r || !r.IsSuccess()) return response;
        var events = unitOfWorkHandler.GetDomainEvents();
        foreach (var e in events)
        {
            var method = typeof(IEventExecutor).GetMethod(nameof(IEventExecutor.ExecuteAsync))!;
            var genericMethod = method.MakeGenericMethod(e.GetType());
            await (Task)genericMethod.Invoke(eventExecutor, [e, cancellationToken])!;
        }
        await unitOfWorkHandler.SaveChangesAsync(cancellationToken);
        try
        {
            foreach (var e in events)
            {
                var method = typeof(IEventExecutor).GetMethod(nameof(IEventExecutor.PostExecuteAsync))!;
                var genericMethod = method.MakeGenericMethod(e.GetType());
                await (Task)genericMethod.Invoke(eventExecutor, [e, cancellationToken])!;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Post Execute Event Error");
        }
        return response;
    }
}