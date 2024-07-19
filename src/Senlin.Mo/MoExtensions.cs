using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;
using Senlin.Mo.Middlewares;
using Senlin.Mo.Repository.Abstractions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace Senlin.Mo;

/// <summary>
/// Mo Application Extensions
/// </summary>
public static class MoExtensions
{
    private static IModule[]? _modules;
    private static readonly MoConfigureOptions Options = new();

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
        configureOptions?.Invoke(Options);
        
        var modules = Options.Modules ?? [];
        services.TryAddSingleton<GetNow>(()=>DateTime.Now);
        services.TryAddScoped<GetTenant>(_ => () => Options.SystemTenant);
        services.TryAddScoped<GetUserId>(_ => () => string.Empty);
        services.TryAddScoped<GetCulture>(sp => sp.GetCulture(Options.LocalizationOptions));
        services.TryAddSingleton<NewConcurrencyToken>(() => Guid.NewGuid().ToByteArray());
        services.TryAddSingleton<GetSystemTenant>(() => Options.SystemTenant);
        services.TryAddScoped<IRepositoryHelper, RepositoryHelper>();

        services
            .AddHttpContextAccessor()
            .AddSingleton<IdGenerator>()
            .ConfigureLog(Options.Logger)
            .ConfigureLocalization(Options.LocalizationOptions)
            .AddFluentValidationAutoValidation();
        if (!Options.Logger.DisableDefaultMiddleware)
        {
            services.AddScoped<LogMiddleware>();
        }

        foreach (var module in modules)
        {
            services.AddModule(module, Options.ModuleOptions);
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
        services.TryAddScoped<IEventExecutor, EventExecutor>();
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
        if (!Options.Logger.DisableDefaultMiddleware)
        {
            app.UseMiddleware<LogMiddleware>();
        }
        app.UseExceptionHandler(exceptionHandlerBuilder ?? ConfigureExceptionHandler);
        app.UseRequestLocalization();
    }

    private static void ConfigureExceptionHandler(IApplicationBuilder b) =>
        b.Run(context => Results.Problem().ExecuteAsync(context));
}