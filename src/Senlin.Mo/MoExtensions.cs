﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
    private static readonly Dictionary<IModule, Type> ModuleDbContextTypes = new();

    /// <summary>
    /// Configure Mo Application
    /// </summary>
    /// <param name="services"></param>
    /// <param name="module"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureMo(
        this IServiceCollection services,
        IModule module,
        Action<MoConfigureOptions>? configureOptions = null) =>
        services.ConfigureMo([module], configureOptions);

    /// <summary>
    /// Configure Mo Application
    /// </summary>
    /// <param name="services"></param>
    /// <param name="modules"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureMo(
        this IServiceCollection services,
        IModule[] modules,
        Action<MoConfigureOptions>? configureOptions = null)
    {
        configureOptions?.Invoke(Options);

        services.TryAddSingleton<GetNow>(() => DateTime.Now);
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
            services.AddModule(module);
        }

        _modules ??= modules;
        return services;
    }

    private static void AddModule(
        this IServiceCollection services,
        IModule module)
    {
        services.AddModuleLStringResolver(module);
        var dbContextType = module.Assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(type => type.IsAssignableTo(typeof(DbContext)));
        if (dbContextType is not null)
        {
            ModuleDbContextTypes[module] = dbContextType;
            services.AddDbContext(dbContextType, module.ConnectionString);
        }
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

    internal static Type GetDbContextType(this IModule module) =>
        ModuleDbContextTypes[module];
    
    private static void ConfigureExceptionHandler(IApplicationBuilder b) =>
        b.Run(context => Results.Problem().ExecuteAsync(context));
}