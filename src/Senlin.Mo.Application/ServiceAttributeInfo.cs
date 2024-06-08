using System.Collections.Immutable;
using Senlin.Mo.Application.Helpers;

namespace Senlin.Mo.Application;

internal readonly record struct ServiceAttributeInfo
{
    public ServiceAttributeInfo(bool isUnitOfWork, string endpoint, string[] methods, string[] patternMatchNames)
    {
      
        IsUnitOfWork = isUnitOfWork;
        Endpoint = endpoint;
        Methods = methods;
        PatternMatchNames = new EquatableArray<string>(patternMatchNames);
    }

    public readonly bool IsUnitOfWork;
    public readonly string Endpoint;
    public readonly string[] Methods;
    public readonly EquatableArray<string> PatternMatchNames;
}
