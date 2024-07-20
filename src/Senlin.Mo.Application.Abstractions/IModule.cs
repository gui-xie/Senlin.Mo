using System.Reflection;

namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Module Interface
/// </summary>
public interface IModule
{
    /// <summary>
    /// Get Services registration
    /// </summary>
    /// <returns></returns>
    IEnumerable<ServiceRegistration> GetServices();
 
    /// <summary>
    /// Get Module Assemblies (for auto registration, etc)
    /// </summary>
    Assembly[] Assemblies { get; }
    
    /// <summary>
    /// Get Localization Path
    /// </summary>
    string LocalizationPath { get; }

    /// <summary>
    /// Get Connection String
    /// </summary>
    string ConnectionString { get; }
}