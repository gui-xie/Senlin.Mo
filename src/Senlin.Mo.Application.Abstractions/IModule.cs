namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Module Interface
/// </summary>
public interface IModule
{
    /// <summary>
    /// Module Name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get Services registration
    /// </summary>
    /// <returns></returns>
    IEnumerable<ServiceRegistration> GetServices();
    
    /// <summary>
    /// Get DbContext Type
    /// </summary>
    Type? DbContextType { get; }
}

