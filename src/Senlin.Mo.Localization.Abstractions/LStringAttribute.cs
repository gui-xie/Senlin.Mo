namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string attribute
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class LStringAttribute(string separator = "_") : Attribute
{
    /// <summary>
    /// The separator of the key
    /// </summary>
    public string Separator { get; } = separator;
}