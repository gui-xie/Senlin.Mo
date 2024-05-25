namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string resolver extensions class
/// </summary>
public static class LStringResolverExtensions
{
    /// <summary>
    /// Resolve the key
    /// </summary>
    /// <param name="resolver">Localization string resolver</param>
    /// <param name="lString">Localization string</param>
    /// <returns></returns>
    public static string Resolve(this LStringResolver resolver, LString lString) 
        => lString.Resolve(resolver.Resolve);
}