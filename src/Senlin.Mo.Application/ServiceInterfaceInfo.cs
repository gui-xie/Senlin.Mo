namespace Senlin.Mo.Application;

internal readonly record struct ServiceInterfaceInfo(string Name, string RequestName, string ResponseName)
{
    public readonly string Name = Name;

    public readonly string RequestName = RequestName;

    public readonly string ResponseName = ResponseName;
}