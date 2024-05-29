using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo;

public class ApplicationConfigureOptions
{
    /// <summary>
    /// Modules
    /// </summary>
    public IModule[]? Modules { get; set; }

    /// <summary>
    /// Module options
    /// </summary>
    public ModuleOptions ModuleOptions { get; set; } = new();

    /// <summary>
    /// Logger options
    /// </summary>
    public LoggerOptions Logger { get; set; } = new();

    /// <summary>
    /// Localization options
    /// </summary>
    public LocalizationOptions LocalizationOptions { get; set; } = new();
}

public class ModuleOptions
{
    public GetModuleLocalizationPath GetLocalizationPath { get; set; } = moduleName => $"L/{moduleName}";

    public GetModuleConnectionString GetModuleConnectionString { get; set; } = moduleName => string.Empty;
}