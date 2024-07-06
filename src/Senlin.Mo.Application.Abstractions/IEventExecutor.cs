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
}