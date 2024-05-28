using Microsoft.Extensions.Configuration;
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

    /// <summary>
    /// Logger options
    /// </summary>
    public ApplicationLoggerOptions Logger { get; set; } = new();

    /// <summary>
    /// Localization options
    /// </summary>
    public ApplicationLocalizationOptions Localization { get; set; } = new();

    /// <summary>
    /// DbContext connection string
    /// </summary>
    public string DbContextConnectionString { get; set; } = string.Empty;
}