using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        
        var modules = builder.Modules ?? [];
        services.TryAddSingleton<GetNow>(() => (EntityDateTime)DateTime.UtcNow);
        services.TryAddScoped<GetTenant>(_ => () => RepositoryHelper.SystemTenant);
        services.TryAddScoped<GetUserId>(_ => () => RepositoryHelper.AdminUser);
        services.TryAddScoped<GetCulture>(sp =>
        {
            var culture = LocalizationExtensions.GetCulture(sp.GetRequiredService<IHttpContextAccessor>());
            return () => culture;
        });
        
        services
            .AddLocalization()
            .AddSingleton<NewConcurrencyToken>(() => Guid.NewGuid().ToByteArray())
            .AddSingleton<IdGenerator>()
            .AddScoped<IRepositoryHelper, RepositoryHelper>()
            .ConfigureLog(builder.Logger)
            .ConfigureLocalization()
            .AddHttpContextAccessor();

        foreach (var module in modules)
        {
            services.AddModule(
                module,
                builder.Localization, 
                builder.DbContextConnectionString);
        }

        _modules ??= modules;
        return services;
    }
    
    private static void AddModule(
        this IServiceCollection services,
        IModule module,
        ApplicationLocalizationOptions localizationOptions,
        string dbContextConnectionString)
    {
        services.AddModuleLStringResolver(module, localizationOptions.GetModuleLocalizationPath(module));
        services.AddDbContext(module, dbContextConnectionString);
        services.AddAppServices(module);
        services.AddRepositories(module);
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