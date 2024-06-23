using System.Collections.Immutable;
using Senlin.Mo.Application.Helpers;

namespace Senlin.Mo.Application;

internal readonly record struct ServiceAttributeInfo
{
    public ServiceAttributeInfo(string endpoint, string method, string[] patternMatchNames, string[] serviceDecorators)
    {
      
        Endpoint = endpoint;
        Method = method;
        PatternMatchNames = new EquatableArray<string>(patternMatchNames);
        ServiceDecorators = new EquatableArray<string>(serviceDecorators);
    }

    public readonly string Endpoint;
    public readonly string Method;
    public readonly EquatableArray<string> PatternMatchNames;
    public readonly EquatableArray<string> ServiceDecorators;
}
