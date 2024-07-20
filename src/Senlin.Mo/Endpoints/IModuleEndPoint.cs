using Microsoft.AspNetCore.Routing;

namespace Senlin.Mo.Endpoints;

public interface IModuleEndPoint
{
    static abstract void Map(IEndpointRouteBuilder builder);
}