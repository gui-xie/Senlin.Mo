using System.Collections.Immutable;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo;

public static class EndpointExtensions
{
    /// <summary>
    /// Map endpoints
    /// </summary>
    /// <param name="group"></param>
    /// <param name="module"></param>
    public static void MapEndpoints(this RouteGroupBuilder group, params IModule[] module)
    {
        foreach (var m in module)
        {
            group.MapEndpoints(m);
        }
    }
    
    /// <summary>
    /// Map endpoints
    /// </summary>
    /// <param name="group"></param>
    /// <param name="module"></param>
    /// <param name="groupName">when null: lower module name</param>
    public static void MapEndpoints(this RouteGroupBuilder group, IModule module, string? groupName = null)
    {
        var gName = groupName ?? module.Name.ToLower();
        var g = group.MapGroup(gName).WithTags(gName);
        var mapper = from s in module.GetServices()
            where s.EndpointData is not null
            select g.MapMethods(
                s.EndpointData.Pattern,
                s.EndpointData.Methods,
                s.EndpointData.Handler);
        mapper.ToImmutableArray();
    }
}