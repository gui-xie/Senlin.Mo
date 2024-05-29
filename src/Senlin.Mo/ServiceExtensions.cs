using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo;

internal static class ServiceExtensions
{
    public static void AddRepositories(this IServiceCollection services, IModule module)
    {
        foreach (var (abstraction, implementation) in module.GetRepositories())
        {
            services.AddTransient(abstraction, implementation);
        }
    }
    
      public static void AddAppServices(this IServiceCollection services, IModule module)
    {
        foreach (var serviceRegistration in module.GetServices())
        {
            services.AddTransient(serviceRegistration.ServiceType, sp =>
            {
                var type = serviceRegistration.Implementation;
                var s = sp.CreateModuleInstance(type, module);
                var serviceGenericType = serviceRegistration.ServiceType.GetGenericArguments();
                foreach (var decorator in serviceRegistration.Decorators)
                {
                    Type decoratorType;
                    if (decorator == typeof(UnitOfWorkDecorator<,,>))
                    {
                        if(module.DbContextType is null)
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
                        args.Add((object)sp.GetRequiredService<GetUserId>());
                    }
                    s = Activator.CreateInstance(
                        decoratorType,
                        args.ToArray())!;
                }

                return s;
            });
        }

    }
    private static object CreateModuleInstance(
        this IServiceProvider sp,
        Type type, 
        IModule module)
    {
        var constructor = type.GetConstructors().First();
        var parameters = constructor.GetParameters();
        var args = parameters.Select(p =>
        {
            var argType = p.ParameterType;
            if (argType == typeof(LStringResolver))
            {
                return module.GetLStringResolverType();
            }
            if (argType == typeof(ILogger))
            {
                return module.GetLoggerType();
            }
            return sp.GetRequiredService(argType);
        }).ToArray();
        return Activator.CreateInstance(type, args)!;
    }


}