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