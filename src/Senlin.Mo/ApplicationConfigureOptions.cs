using Microsoft.Extensions.Configuration;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;
using Senlin.Mo.Module;

namespace Senlin.Mo;

public class ApplicationConfigureOptions
{
    /// <summary>
    /// Modules
    /// </summary>
    public IModule[]? Modules { get; set; }
    
    /// <summary>
    /// Configuration
    /// </summary>
    public IConfiguration? Configuration { get; set; }
}