namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Service Route Data
/// </summary>
/// <param name="Pattern"></param>
/// <param name="Handler"></param>
/// <param name="Methods"></param>
public record EndpointData(string Pattern, Delegate Handler, params string[] Methods)
{
    /// <summary>
    /// Route Pattern
    /// </summary>
    public string Pattern { get; } = Pattern;
    
    /// <summary>
    /// Route Handler
    /// </summary>
    public Delegate Handler { get; } = Handler;
    
    /// <summary>
    /// Route Methods
    /// </summary>
    public string[] Methods { get; } = Methods;
}