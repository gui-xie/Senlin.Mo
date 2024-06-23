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
    /// <param name="method"></param>
    public ServiceEndpointAttribute(string pattern, string method = "")
    {
        Pattern = pattern;
        Method = method;
    }

    /// <summary>
    /// Pattern
    /// </summary>
    public string Pattern { get; }
    
    /// <summary>
    /// Methods
    /// </summary>
    public string Method { get; }
}