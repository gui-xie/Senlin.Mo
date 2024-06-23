namespace Senlin.Mo.Application.Abstractions.Decorators.Log;

/// <summary>
/// Log attribute
/// </summary>
public class LogAttribute: Attribute, IServiceDecorator
{
    /// <summary>
    /// Decorator Service Type
    /// </summary>
    public Type ServiceType => typeof(LogService<,>);

    /// <summary>
    /// Configure
    /// </summary>
    /// <param name="service"></param>
    public void Configure(IService? service)
    {
        
    }
}