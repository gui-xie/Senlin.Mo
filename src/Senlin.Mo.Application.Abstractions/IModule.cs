using System.Reflection;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Module Interface
/// </summary>
public interface IModule
{
    /// <summary>
    /// Get Services registration
    /// Senlin.Mo will auto registration the following services
    /// class name end with "Service" and implement IService<,> or ICommandService<>
    /// class name end with "Repository" and implement interface name end with Repository
    /// class name end with "Validator" and implement IVaildator<>
    /// class name end with "EventHandler" and implement IEventHandler<> or IPostEventHandler<>
    /// </summary>
    IEnumerable<ServiceRegistration> GetServices();
 
    /// <summary>
    /// Get Localization Path
    /// </summary>
    string LocalizationPath { get; }

    /// <summary>
    /// Get Connection String
    /// </summary>
    string ConnectionString { get; }
}