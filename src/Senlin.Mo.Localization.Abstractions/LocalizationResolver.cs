using System.Collections.Concurrent;
using System.Text.Json;

namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization resolver
/// </summary>
/// <param name="culture"></param>
public class LocalizationResolver(string culture, string directory)
{
    private static readonly ConcurrentDictionary<string, Lazy<Dictionary<string, string>>> Dictionaries
        = new();
    
    /// <summary>
    /// Resolve the key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Resolve(string key)
    {
        var dict = Dictionaries.GetOrAdd(culture, _ => new Lazy<Dictionary<string, string>>(() =>
        {
            var json = File.ReadAllText(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    directory, 
                    $"{culture}.json"));
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
        }));
        var v = dict.Value[key];
        return v;
    }
}