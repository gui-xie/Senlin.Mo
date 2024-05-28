using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Module;
using Senlin.Mo.Repository.Abstractions;

namespace Senlin.Mo;

internal static class DbContextExtensions
{
    public static void AddDbContext(
        this IServiceCollection services,
        IModule module,
        string connectionStringValue)
    {
        var dbContextType = module.DbContextType;
        if(dbContextType is null) return;
        var connectionStringType = typeof(ConnectionString<>).MakeGenericType(dbContextType);
        var connectionString = Activator.CreateInstance(connectionStringType, connectionStringValue)!;
        services.AddSingleton(connectionStringType, connectionString);
        (from method in typeof(EntityFrameworkServiceCollectionExtensions)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public)
                let parameters = method.GetParameters()
                where method.Name == nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext)
                      && method.IsGenericMethod
                      && method.GetGenericArguments().Length == 1
                      && parameters.Length == 4
                      && parameters[0].ParameterType == typeof(IServiceCollection)
                      && parameters[1].ParameterType == typeof(Action<DbContextOptionsBuilder>)
                      && parameters[2].ParameterType == typeof(ServiceLifetime)
                      && parameters[3].ParameterType == typeof(ServiceLifetime)
                select method.MakeGenericMethod(dbContextType))
            .First()
            .Invoke(null, [services, null, ServiceLifetime.Scoped, ServiceLifetime.Scoped]);
    }
}