namespace Senlin.Mo;

public class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "en";

    public string CultureQueryStringKey { get; set; } = "culture";

    public string CultureHeaderName { get; set; } = "Mo-Culture";
}