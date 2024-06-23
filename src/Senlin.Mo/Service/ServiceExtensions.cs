using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo;

internal static class ServiceExtensions
{
    public static void AddAppServices(this IServiceCollection services, IModule module)
    {
        foreach (var serviceRegistration in module.GetServices())
        {
            if (!(serviceRegistration.ServiceType.IsGenericType 
                  && serviceRegistration.ServiceType.Name.StartsWith("IService")
                && serviceRegistration.ServiceType.GetGenericTypeDefinition() == typeof(IService<,>)))
            {
                var serviceDescription = new ServiceDescriptor(serviceRegistration.ServiceType, serviceRegistration.Implementation, serviceRegistration.LifeTime);
                services.Add(serviceDescription);
                continue;
            }
            services.AddTransient(serviceRegistration.ServiceType, sp =>
            {
                var type = serviceRegistration.Implementation;
                var s = (IService)sp.CreateModuleInstance(type, module);
                var decorators = serviceRegistration.Decorators ?? [];
                foreach (var decorator in decorators)
                {
                    s = sp.DecorateService(
                        decorator.ServiceType, 
                        serviceRegistration.ServiceType,
                        s, 
                        module);
                    decorator.Configure(s);
                }
                return s;
            });
        }
    }

    private static object GetModuleRequiredService(this IServiceProvider sp, IModule module, Type type)
    {
        if (type == typeof(ILStringResolver))
        {
            type = module.GetLStringResolverType();
        }
        else if (type == typeof(IUnitOfWorkHandler))
        {
            type = module.DbContextType!;
        }
        return sp.GetRequiredService(type);
    }

    private static IService DecorateService(
        this IServiceProvider sp,
        Type decoratorGenericType,
        Type serviceType,
        object service,
        IModule module)
    {
        var serviceGenericArguments = serviceType.GetGenericArguments();
        var decoratorType = decoratorGenericType.MakeGenericType(serviceGenericArguments);
        var args =  decoratorType
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => 
                p.ParameterType == serviceType 
                    ? service 
                    : sp.GetModuleRequiredService(module, p.ParameterType))
            .ToArray();
        return (Activator.CreateInstance(
            decoratorType, 
            args) as IService)!;   
    }
    
    private static object CreateModuleInstance(
        this IServiceProvider sp,
        Type type,
        IModule module)
    {
        var args =  type
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => sp.GetModuleRequiredService(module, p.ParameterType))
            .ToArray();
        return Activator.CreateInstance(
            type, 
            args)!;
    }
}