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
    public ServiceRegistration(
        Type serviceType,
        Type implementation,
        IServiceDecorator[]? decorators = null)
    {
        ServiceType = serviceType;
        Implementation = implementation;
        Decorators = decorators;
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
    /// Service Decorator
    /// </summary>
    public IServiceDecorator[]? Decorators { get; }
}