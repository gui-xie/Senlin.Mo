namespace Senlin.Mo;

public class ModuleOptions
{
    public GetModuleLocalizationPath GetLocalizationPath { get; set; } = _ => $"L";

    public GetModuleConnectionString GetModuleConnectionString { get; set; } = _ => throw new NotImplementedException();
}