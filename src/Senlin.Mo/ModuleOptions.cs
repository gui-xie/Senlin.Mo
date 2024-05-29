namespace Senlin.Mo;

public class ModuleOptions
{
    public GetModuleLocalizationPath GetLocalizationPath { get; set; } = moduleName => $"L/{moduleName}";

    public GetModuleConnectionString GetModuleConnectionString { get; set; } = moduleName => string.Empty;
}
