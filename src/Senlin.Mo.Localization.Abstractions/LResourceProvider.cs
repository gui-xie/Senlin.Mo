namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string Resource
/// </summary>
public class LResourceProvider
{
    private readonly Dictionary<string, ILResource> _resources;

    /// <summary>
    /// Localization string Resource Provider
    /// </summary>
    /// <param name="resources"></param>
    public LResourceProvider(params ILResource[] resources)
    {
        _resources = resources.ToDictionary(x => x.Culture);
    }

    /// <summary>
    /// Get Resource
    /// </summary>
    /// <param name="culture">Culture</param>
    /// <returns></returns>
    public Dictionary<string, string> GetResource(string culture)
    {
        _resources.TryGetValue(culture, out var resource);
        return resource is null?
            new Dictionary<string, string>()
            : resource.GetResource();
    } 
}