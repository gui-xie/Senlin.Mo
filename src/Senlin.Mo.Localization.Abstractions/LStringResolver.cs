using System.Collections.Concurrent;

namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string resolver
/// </summary>
/// <param name="getCulture">Get current culture</param>
/// <param name="getCultureResource">get culture resource</param>
public class LStringResolver(
    GetCulture getCulture,
    GetCultureResource getCultureResource): ILStringResolver
{
    private static readonly ConcurrentDictionary<string, Lazy<Dictionary<string, string>>> Dictionaries = new();

    internal string Resolve(string key)
    {
        var culture = getCulture();
        var dict = Dictionaries.GetOrAdd(
            culture,
            _ => new Lazy<Dictionary<string, string>>(() 
                => getCultureResource(culture)));
        if (dict.Value is null) return string.Empty;
        dict.Value.TryGetValue(key, out var v);
        return v ?? string.Empty;
    }

    /// <summary>
    /// Resolve localization string
    /// </summary>
    /// <param name="lString"></param>
    public string this[LString lString] => this.Resolve(lString);
}


/// <summary>
/// Localization string resolver
/// </summary>
/// <param name="getCulture"></param>
/// <param name="getCultureResource"></param>
/// <typeparam name="T"></typeparam>
public class LStringResolver<T>(
    GetCulture getCulture,
    GetCultureResource getCultureResource) 
    : LStringResolver(getCulture, getCultureResource)
{
        
}