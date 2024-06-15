using System.Collections.Immutable;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo;

internal static class ServiceExtensions
{
    public static void AddAppServices(this IServiceCollection services, IModule module)
    {
        foreach (var serviceRegistration in module.GetServices())
        {
            if (!(serviceRegistration.ServiceType.IsGenericType && serviceRegistration.ServiceType.Name.StartsWith("IService")
                && serviceRegistration.ServiceType.GetGenericTypeDefinition() == typeof(IService<,>)))
            {
                var serviceDescription = new ServiceDescriptor(serviceRegistration.ServiceType, serviceRegistration.Implementation, serviceRegistration.LifeTime);
                services.Add(serviceDescription);
                continue;
            }
            services.AddTransient(serviceRegistration.ServiceType, sp =>
            {
                var type = serviceRegistration.Implementation;
                var s = sp.CreateModuleInstance(type, module);
                var serviceGenericType = serviceRegistration.ServiceType.GetGenericArguments();
                var decorators = serviceRegistration.Decorators ?? [];
                foreach (var decorator in decorators)
                {
                    Type decoratorType;
                    if (decorator == typeof(UnitOfWorkDecorator<,,>))
                    {
                        if (module.DbContextType is null)
                        {
                            continue;
                        }

                        var unitOfWorkGenericTypeArguments = serviceGenericType
                            .Concat([module.DbContextType])
                            .ToArray();
                        decoratorType = decorator.MakeGenericType(unitOfWorkGenericTypeArguments);
                        s = Activator.CreateInstance(
                            decoratorType,
                            s,
                            sp.GetRequiredService(module.DbContextType))!;
                        continue;
                    }

                    if (decorator.GetGenericArguments().Length != 2) continue;
                    decoratorType = decorator.MakeGenericType(serviceGenericType);
                    var args = new List<object> { s };
                    if (decorator == typeof(LogDecorator<,>))
                    {
                        args.Add(sp.GetRequiredService(typeof(ILogger<>).MakeGenericType(serviceGenericType[0])));
                        args.Add(sp.GetRequiredService<GetUserId>());
                    }
                    s = Activator.CreateInstance(
                        decoratorType,
                        args.ToArray())!;
                }

                return s;
            });
        }
    }

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

    private static object GetModuleRequiredService(this IServiceProvider sp, IModule module, Type type)
    {
        if (type == typeof(LStringResolver))
        {
            type = module.GetLStringResolverType();
        }
        else if (type == typeof(ILogger))
        {
            type = module.GetLoggerType();
        }
        return sp.GetRequiredService(type);
    }

    private static object CreateModuleInstance(
        this IServiceProvider sp,
        Type type,
        IModule module)
    {
        var args =  type
            .GetConstructors()
            .First().GetParameters()
            .Select(p => sp.GetModuleRequiredService(module, p.ParameterType))
            .ToArray();
        return Activator.CreateInstance(
            type, 
            args)!;
    }
}