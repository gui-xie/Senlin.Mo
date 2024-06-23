using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Application.Abstractions.Decorators;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Service registration
/// </summary>
public class ServiceRegistration
{
    /// <summary>
    /// Service registration
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="implementation"></param>
    /// <param name="decorators"></param>
    /// <param name="lifeTime"></param>
    /// <param name="endpointData"></param>
    public ServiceRegistration(
        Type serviceType,
        Type implementation,
        IServiceDecorator[]? decorators = null,
        ServiceLifetime lifeTime = ServiceLifetime.Transient,
        EndpointData? endpointData = null)
    {
        ServiceType = serviceType;
        Implementation = implementation;
        Decorators = decorators;
        LifeTime = lifeTime;
        EndpointData = endpointData;
    }

    /// <summary>
    /// Service Type
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Service Implementation
    /// </summary>
    public Type Implementation { get; }

    /// <summary>
    /// Service LifeCycle
    /// </summary>
    public ServiceLifetime LifeTime { get; }

    /// <summary>
    /// Service Decorator
    /// </summary>
    public IServiceDecorator[]? Decorators { get; }

    /// <summary>
    /// Route Pattern, Handler, Methods
    /// </summary>
    public EndpointData? EndpointData { get; }
}