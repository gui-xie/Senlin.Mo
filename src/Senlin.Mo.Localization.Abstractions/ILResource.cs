namespace Senlin.Mo.Localization.Abstractions;

/// <summary>
/// Localization string Resource
/// </summary>
public interface ILResource
{
    /// <summary>
    /// The culture of the resource
    /// </summary>
    string Culture { get; }
    
    /// <summary>
    /// Get Resource
    /// </summary>
    /// <returns></returns>
    Dictionary<string, string> GetResource();
}