using System.Collections.Immutable;
using Senlin.Mo.Application.Helpers;

namespace Senlin.Mo.Application;

internal readonly record struct ServiceAttributeInfo
{
    public ServiceAttributeInfo(string endpoint, string method, string[] queryParameters, string[] serviceDecorators)
    {
        Endpoint = endpoint;
        Method = method;
        QueryParameters = new EquatableArray<string>(queryParameters);
        ServiceDecorators = new EquatableArray<string>(serviceDecorators);
    }

    public readonly string Endpoint;
    public readonly string Method;
    public readonly EquatableArray<string> QueryParameters;
    public readonly EquatableArray<string> ServiceDecorators;
}
