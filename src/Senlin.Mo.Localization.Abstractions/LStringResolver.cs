using System.Collections.Concurrent;

namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string resolver
/// </summary>
/// <param name="getCulture"></param>
/// <param name="getCultureResource"></param>
public class LStringResolver(
    GetCulture getCulture,
    GetCultureResource getCultureResource,
    bool isCultureWillChanged = true)
{
    private static readonly ConcurrentDictionary<string, Lazy<Dictionary<string, string>>> Dictionaries = new();
    private readonly string _culture = getCulture();

    internal string Resolve(string key)
    {
        var culture = isCultureWillChanged ? getCulture() : _culture;
        var dict = Dictionaries.GetOrAdd(
            culture,
            _ => new Lazy<Dictionary<string, string>>(() 
                => getCultureResource(culture)));
        if (dict.Value is null) return string.Empty;
        dict.Value.TryGetValue(key, out var v);
        return v ?? string.Empty;
    }
}