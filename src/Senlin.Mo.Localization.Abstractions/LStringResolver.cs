using System.Collections.Concurrent;

namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string resolver
/// </summary>
/// <param name="getCulture"></param>
/// <param name="getCultureResource"></param>
public class LStringResolver(
    GetCulture getCulture,
    GetCultureResource getCultureResource)
{
    private static readonly ConcurrentDictionary<string, Lazy<Dictionary<string, string>>> Dictionaries = new();
    private readonly string _culture = getCulture();

    /// <summary>
    /// Resolve the key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Resolve(string key)
    {
        var dict = Dictionaries.GetOrAdd(
            _culture,
            _ => new Lazy<Dictionary<string, string>>(()
                => getCultureResource(_culture)));
        var v = dict.Value[key];
        return v;
    }
}