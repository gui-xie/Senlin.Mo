using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;

namespace Senlin.Mo;

internal class EventExecutor(IServiceProvider sp) : IEventExecutor
{
    public async Task ExecuteAsync<T>(T e, CancellationToken cancellationToken) where T : IDomainEvent
    {
        var handlers = sp.GetServices<IEventHandler<T>>();
        foreach (var handler in handlers)
        {
            await handler.ExecuteAsync(e, cancellationToken);
        }
    }
}