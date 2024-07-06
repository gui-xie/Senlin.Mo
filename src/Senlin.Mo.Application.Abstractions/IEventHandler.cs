using Senlin.Mo.Domain;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Event Handler
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventHandler<in T> where T: IDomainEvent
{
    /// <summary>
    /// Process Event
    /// </summary>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(T e, CancellationToken cancellationToken);
}