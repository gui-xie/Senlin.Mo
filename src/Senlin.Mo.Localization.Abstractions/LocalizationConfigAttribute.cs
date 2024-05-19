namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization config
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class LocalizationConfigAttribute : Attribute
{
    /// <summary>
    ///  The path of localization file
    /// </summary>
    public string Path { get; set; } = LocalizationConfigExtensions.DefaultPath;

    /// <summary>
    ///  The culture of localization file
    /// </summary>
    public string Culture { get; set; } = LocalizationConfigExtensions.DefaultCulture;
}