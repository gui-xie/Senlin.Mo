using Senlin.Mo.Application.Helpers;

namespace Senlin.Mo.Application;

internal readonly record struct ServiceInfo
{
    public ServiceInfo
    (
        string serviceName,
        string serviceNamespace,
        ServiceInterfaceInfo serviceInterfaceInfo,
        ServiceAttributeInfo serviceAttributeInfo,
        ServiceCategory serviceCategory,
        TypeProperty[] requestProperties,
        bool isRequestRecord)
    {
        ServiceName = serviceName;
        ServiceInterfaceInfo = serviceInterfaceInfo;
        ServiceNamespace = serviceNamespace;
        ServiceCategory = serviceCategory;
        IsEnableUnitOfWork = serviceAttributeInfo.IsUnitOfWork;
        Endpoint = serviceAttributeInfo.Endpoint;
        Methods = new EquatableArray<string>(serviceAttributeInfo.Methods);
        RequestProperties = new EquatableArray<TypeProperty>(requestProperties);
        IsRequestRecord = isRequestRecord;
        PatternMatchNames = serviceAttributeInfo.PatternMatchNames;
    }

    public readonly string ServiceName;
    public readonly string ServiceNamespace;
    public readonly ServiceInterfaceInfo ServiceInterfaceInfo;
    public readonly ServiceCategory ServiceCategory;
    public readonly bool IsEnableUnitOfWork;
    public readonly string Endpoint;
    public readonly EquatableArray<string> Methods;
    public readonly EquatableArray<TypeProperty> RequestProperties;
    public readonly bool IsRequestRecord;
    public readonly EquatableArray<string> PatternMatchNames;
}