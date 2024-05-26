using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;
using Senlin.Mo.Module;

namespace Senlin.Mo;

public class ApplicationConfigureOptions
{
    /// <summary>
    /// GetTenant function
    /// </summary>
    public Func<IServiceProvider, GetTenant>? GetTenant { get; set; }

    /// <summary>
    /// GetUserId function
    /// </summary>
    public Func<IServiceProvider, GetUserId>? GetUserId { get; set; }


    /// <summary>
    /// Get culture function
    /// </summary>
    public Func<IServiceProvider, GetCulture>? GetCulture { get; set; }

    /// <summary>
    /// Modules
    /// </summary>
    public IModule[]? Modules { get; set; }
}