namespace Senlin.Mo.Application.Abstractions.Decorators;

/// <summary>
/// Service Decorator
/// </summary>
public interface IServiceDecorator
{
    /// <summary>
    /// Service Type
    /// </summary>
    Type ServiceType { get; }

    /// <summary>
    /// Configure attribute data to service
    /// </summary>
    void Configure(IService? service);
}