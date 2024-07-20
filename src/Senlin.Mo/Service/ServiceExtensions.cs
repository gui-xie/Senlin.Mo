using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Application.Abstractions.Decorators;

namespace Senlin.Mo;

internal static class ServiceExtensions
{
    private static readonly Dictionary<Type, Type> DecoratorServices = new();
    
    internal static void AddAppServices(this IServiceCollection services, IModule module)
    {
        foreach (var serviceRegistration in module.GetServices())
        {
            if (!(serviceRegistration.ServiceType.IsGenericType 
                  && serviceRegistration.ServiceType.Name.StartsWith("IService")
                && serviceRegistration.ServiceType.GetGenericTypeDefinition() == typeof(IService<,>)))
            {
                var serviceDescription = new ServiceDescriptor(
                    serviceRegistration.ServiceType, 
                    serviceRegistration.Implementation, 
                    ServiceLifetime.Transient);
                services.Add(serviceDescription);
                continue;
            }
            
            services.AddTransient(
                serviceRegistration.ServiceType,
                sp =>
            {
                var type = serviceRegistration.Implementation;
                var s = (IService)sp.CreateModuleInstance(type, module);
                var decorators = serviceRegistration.Decorators ?? [];
                return decorators.Aggregate(s, (current, decorator) 
                    => sp.DecorateService(
                        decorator,
                        serviceRegistration.ServiceType,
                        current,
                        module));
            });
        }
    }
    
    private static Type GetDecoratorServiceType(Type decoratorType)
    {
        if (DecoratorServices.TryGetValue(decoratorType, out var serviceType))
        {
            return serviceType;
        }

        var decoratorServiceType = typeof(IDecoratorService<>).MakeGenericType(decoratorType);
        var types = 
            from t in decoratorType.Assembly.GetTypes()
            where t.IsAssignableTo(decoratorServiceType)
            select t;
        return DecoratorServices[decoratorType] = types.First();
    }
    
    private static object GetModuleRequiredService(this IServiceProvider sp, IModule module, Type type)
    {
        if (type == typeof(IUnitOfWorkHandler))
        {
            type = module.GetDbContextType();
        }

        return sp.GetRequiredService(type);
    }

    private static IService DecorateService(
        this IServiceProvider sp,
        object decorator,
        Type serviceType,
        object service,
        IModule module)
    {
        var serviceGenericArguments = serviceType.GetGenericArguments();
        var decoratorGenericServiceType = GetDecoratorServiceType(decorator.GetType());
        var decoratorServiceType = decoratorGenericServiceType.MakeGenericType(serviceGenericArguments);
        var args =  decoratorServiceType
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => 
                p.ParameterType == serviceType 
                    ? service 
                    : sp.GetModuleRequiredService(module, p.ParameterType))
            .ToArray();
        var decoratedService = Activator.CreateInstance(
            decoratorServiceType,
            args)!;
        // property inject
        decoratorServiceType.GetProperty("AttributeData")!
            .SetValue(decoratedService, decorator); 
        return (IService)decoratedService;
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