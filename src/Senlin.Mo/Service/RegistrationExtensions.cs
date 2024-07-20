using System.Reflection;
using FluentValidation;
using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo;

internal static class RegistrationExtensions
{
    public static IEnumerable<ServiceRegistration> GetServiceRegistrations(params Assembly[] assemblies)
    {
        var services = from type in assemblies.SelectMany(a => a.GetTypes())
            where type.IsClass && type.Name.EndsWith("ServiceImpl")
            select type;
        foreach (var service in services)
        {
            var serviceInterface = service.GetInterfaces().First();
            var registration = serviceInterface.GetField("Registration", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);
            if (registration is null) continue;
            yield return (ServiceRegistration)registration;
            yield return new ServiceRegistration(serviceInterface, service);
        }
    }

    public static IEnumerable<ServiceRegistration> GetRepositoryRegistrations(params Assembly[] assemblies)
    {
        var repositories =
            from type in assemblies.SelectMany(a => a.GetTypes())
            where type.IsClass && !type.IsAbstract && type.Name.EndsWith("Repository")
            let interfaces = type.GetInterfaces().Where(i => i.Name.EndsWith("Repository"))
            from i in interfaces
            select new ServiceRegistration(i, type);
        return repositories;
    }

    public static IEnumerable<ServiceRegistration> GetValidatorRegistrations(params Assembly[] assemblies)
    {
        var validators = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && t.Name.EndsWith("Validator")
                        && t.IsAssignableTo(typeof(IValidator)))
            .Select(x =>
            {
                var service = x.GetInterfaces().First(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
                return new ServiceRegistration(service, x);
            });
        return validators;
    }
    
    public static IEnumerable<ServiceRegistration> GetEventHandlersRegistrations(params Assembly[] assemblies)
    {
        var eventHandlers = 
            from type in assemblies.SelectMany(a => a.GetTypes())
            where type.IsClass && !type.IsAbstract && type.Name.EndsWith("EventHandler")
            let interfaces = type.GetInterfaces().Where(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IEventHandler<>)
                 || i.GetGenericTypeDefinition() == typeof(IPostEventHandler<>)))
            from i in interfaces
            select new ServiceRegistration(i, type);
        return eventHandlers;
    }
}