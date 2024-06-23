using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;
using Senlin.Mo.Repository.Abstractions;

namespace Senlin.Mo;

/// <summary>
/// Mo Application Extensions
/// </summary>
public static class MoExtensions
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
        Action<MoConfigureOptions>? configureOptions = null)
    {
        var options = new MoConfigureOptions();
        configureOptions?.Invoke(options);
        
        var modules = options.Modules ?? [];
        services.TryAddSingleton<GetNow>(()=>DateTimeOffset.Now.ToUnixTimeSeconds());
        services.TryAddSingleton<GetToday>(()=>DateTime.Today);
        services.TryAddScoped<GetTenant>(_ => () => options.SystemTenant);
        services.TryAddScoped<GetUserId>(_ => () => string.Empty);
        services.TryAddScoped<GetCulture>(sp => sp.GetCulture(options.LocalizationOptions.DefaultCulture));
        services.TryAddSingleton<NewConcurrencyToken>(() => Guid.NewGuid().ToByteArray());
        services.TryAddSingleton<GetSystemTenant>(() => options.SystemTenant);
        services.TryAddScoped<IRepositoryHelper, RepositoryHelper>();

        services
            .AddHttpContextAccessor()
            .AddLocalization()
            .AddSingleton<IdGenerator>()
            .ConfigureLog(options.Logger)
            .ConfigureLocalization(options.LocalizationOptions);

        foreach (var module in modules)
        {
            services.AddModule(module, options.ModuleOptions);
        }

        _modules ??= modules;
        return services;
    }
    
    private static void AddModule(
        this IServiceCollection services,
        IModule module,
        ModuleOptions options)
    {
        services.AddModuleLStringResolver(module, options.GetLocalizationPath(module.Name));
        services.AddDbContext(module, options.GetModuleConnectionString(module.Name));
        services.AddAppServices(module);
    }

    /// <summary>
    /// Use Mo
    /// </summary>
    /// <param name="app"></param>
    /// <param name="exceptionHandlerBuilder"></param>
    public static void UseMo(
        this WebApplication app,
        Action<IApplicationBuilder>? exceptionHandlerBuilder = null)
    {
        app.UseExceptionHandler(exceptionHandlerBuilder ?? ConfigureExceptionHandler);
    }

    private static void ConfigureExceptionHandler(IApplicationBuilder b) =>
        b.Run(context => Results.Problem().ExecuteAsync(context));
}