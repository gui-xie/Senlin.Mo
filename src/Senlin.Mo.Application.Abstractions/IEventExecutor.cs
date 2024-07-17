using Senlin.Mo.Domain;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Event handler executor
/// </summary>
public interface IEventExecutor
{
    /// <summary>
    /// Execute event handler
    /// </summary>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task ExecuteAsync<T>(T e, CancellationToken cancellationToken) where T : IDomainEvent;
    
    /// <summary>
    /// Execute event handler(After unit of work)
    /// </summary>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PostExecuteAsync<T>(T e, CancellationToken cancellationToken) where T : IDomainEvent;
}