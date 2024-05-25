namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string key attribute
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class LStringKeyAttribute : Attribute
{
    /// <summary>
    /// Localization string key
    /// </summary>
    /// <param name="key"></param>
    public LStringKeyAttribute(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Localization string key
    /// </summary>
    public string Key { get; set; }
}