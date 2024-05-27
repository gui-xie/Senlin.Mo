using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Senlin.Mo.Localization.Abstractions;
using Senlin.Mo.Module;
using Senlin.Mo.Repository.Abstractions;

namespace Senlin.Mo;

/// <summary>
/// Mo Application Extensions
/// </summary>
public static class ApplicationExtensions
{
    private static IModule[]? _modules;

    /// <summary>
    /// Configure Mo Application
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureMo(
        this IServiceCollection services,
        Action<ApplicationConfigureOptions> configureOptions)
    {
        var builder = new ApplicationConfigureOptions();
        configureOptions(builder);
        var configuration = builder.Configuration;
        const string logPrefix = "Mo:Log:";
        var countLimit = configuration?.GetValue<int>($"{logPrefix}CountLimit") ?? 14;
        var path = configuration?.GetValue<string>($"{logPrefix}Path") ?? "logs";
        var level = configuration?.GetValue<string>($"{logPrefix}Level") ?? "Debug";
        var logConfig = new LogConfig(path, countLimit, level);
        
        var modules = builder.Modules ?? [];
        services.TryAddSingleton<GetNow>(() => (EntityDateTime)DateTime.UtcNow);
        services.TryAddScoped<GetTenant>(sp => () => RepositoryHelper.SystemTenant);
        services.TryAddScoped<GetUserId>(sp => () => RepositoryHelper.AdminUser);
        services.TryAddScoped<GetCulture>(sp =>
        {
            var culture = LocalizationExtensions.GetCulture(sp.GetRequiredService<IHttpContextAccessor>());
            return () => culture;
        });
        
        services
            .AddSwaggerGen()
            .AddEndpointsApiExplorer()
            .AddLocalization()
            .AddSingleton<NewConcurrencyToken>(() => Guid.NewGuid().ToByteArray())
            .AddSingleton<IdGenerator>()
            .AddScoped<IRepositoryHelper, RepositoryHelper>()
            .ConfigureLog(logConfig)
            .ConfigureLocalization()
            .AddHttpContextAccessor();

        foreach (var module in modules)
        {
            services.AddModule(module, builder.Configuration);
        }

        _modules ??= modules;
        return services;
    }

    private static void AddModule(
        this IServiceCollection services,
        IModule module,
        IConfiguration? configuration)
    {
        var lResourceType = module.LStringResolverType;
        var moduleConfigNamePrefix = $"Mo:Modules:{module.Name}:";
        var localizationPathConfigName = $"{moduleConfigNamePrefix}LocalizationPath";
        var connectionStringConfigName = $"{moduleConfigNamePrefix}ConnectionString";
        var localizationDirectory = configuration?.GetValue<string>(localizationPathConfigName) ?? $"L/{module.Name}";
        var connectionStringValue = configuration?[connectionStringConfigName];
        if(connectionStringValue is null)
        {
            throw new ArgumentNullException($"Connection string for module {module.Name} is not found.");
        }
        
        var getResource = GetJsonFileResourcesFn(localizationDirectory);
        services.AddScoped(lResourceType, sp =>
        {
            var getCulture = sp.GetRequiredService<GetCulture>();
            return Activator.CreateInstance(lResourceType, getCulture, getResource)!;
        });

        var dbContextType = module.DbContextType;
        var connectionStringType = typeof(ConnectionString<>).MakeGenericType(dbContextType);
        var connectionString = Activator.CreateInstance(connectionStringType, connectionStringValue)!;
        services.AddSingleton(connectionStringType, connectionString);
        services.AddDbContext(module.DbContextType);

        foreach (var serviceRegistration in module.GetServices())
        {
            services.AddTransient(serviceRegistration.ServiceType, sp =>
            {
                var type = serviceRegistration.Implementation;
                var s = sp.CreateInstance(type);
                var serviceGenericType = serviceRegistration.ServiceType.GetGenericArguments();
                foreach (var decorator in serviceRegistration.Decorators)
                {
                    Type decoratorType;
                    if (decorator == typeof(UnitOfWorkDecorator<,,>))
                    {
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

        foreach (var (abstraction, implementation) in module.GetRepositories())
        {
            services.AddTransient(abstraction, implementation);
        }
    }

    private static object CreateInstance(this IServiceProvider sp, Type type)
    {
        var constructor = type.GetConstructors().First();
        var parameters = constructor.GetParameters();
        var args = parameters.Select(p =>
        {
            var argType = p.ParameterType;
            sp.GetRequiredService(argType);
            return sp.GetRequiredService(argType);
        }).ToArray();
        return Activator.CreateInstance(type, args)!;
    }

    private static void AddDbContext(
        this IServiceCollection services,
        Type dbContextType)
    {
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

    private static GetCultureResource GetJsonFileResourcesFn(
        string localizationPath) =>
        culture =>
        {
            var jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                localizationPath,
                $"{culture}.json");
            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
        };

    /// <summary>
    /// Use Mo
    /// </summary>
    /// <param name="app"></param>
    /// <param name="exceptionHandlerBuilder"></param>
    public static void UseMo(
        this WebApplication app,
        Action<IApplicationBuilder>? exceptionHandlerBuilder = null)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler(exceptionHandlerBuilder ?? ConfigureExceptionHandler);
    }

    private static void ConfigureExceptionHandler(IApplicationBuilder b) =>
        b.Run(context => Results.Problem().ExecuteAsync(context));
}