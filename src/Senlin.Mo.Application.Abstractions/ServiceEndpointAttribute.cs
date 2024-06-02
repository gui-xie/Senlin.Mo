namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Endpoint Attribute
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ServiceEndpointAttribute: Attribute
{
    /// <summary>
    /// Endpoint Attribute
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="methods"></param>
    public ServiceEndpointAttribute(string pattern, params string[] methods)
    {
        Pattern = pattern;
        Methods = methods;
    }

    /// <summary>
    /// Pattern
    /// </summary>
    public string Pattern { get; }
    
    /// <summary>
    /// Methods
    /// </summary>
    public string[] Methods { get; }
}