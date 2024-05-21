using System.Collections.Concurrent;

namespace Senlin.Mo;

/// <summary>
/// Localization resolver
/// </summary>
/// <param name="culture"></param>
public class LocalizationResolver(
    string culture,
    Func<string, Dictionary<string, string>> getCultureResource)
{
    private static readonly ConcurrentDictionary<string, Lazy<Dictionary<string, string>>> Dictionaries = new();

    /// <summary>
    /// Resolve the key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Resolve(string key)
    {
        var dict = Dictionaries.GetOrAdd(
            culture,
            _ => new Lazy<Dictionary<string, string>>(()
                => getCultureResource(culture)));
        var v = dict.Value[key];
        return v;
    }
}