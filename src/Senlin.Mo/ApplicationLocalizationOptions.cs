using Senlin.Mo.Module;

namespace Senlin.Mo;

public class ApplicationLocalizationOptions
{
    public string DefaultCulture { get; set; } = "en";

    public string CultureQueryStringKey { get; set; } = "culture";

    public string CultureHeaderName { get; set; } = "Mo-Culture";

    public Func<IModule, string> GetModuleLocalizationPath { get; set; } = module => $"L/{module.Name}";
}