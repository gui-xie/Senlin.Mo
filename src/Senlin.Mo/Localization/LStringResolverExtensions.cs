using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo;

internal static class LStringResolverExtensions
{
    public static Type GetLStringResolverType(this IModule module) => GetLStringResolverType(module.GetType());

    private static Type GetLStringResolverType(Type type) => typeof(ILStringResolver<>).MakeGenericType(type);

    private static Type? GetModuleResolverType(IModule module)
    {
        var resolverInterfaces =
            from t in module.Assemblies.SelectMany(t => t.GetTypes())
            where t.IsAssignableTo(typeof(ILStringResolver)) && t.IsInterface
            select t;
        var resolver = resolverInterfaces.FirstOrDefault();
        return resolver;
    }

    public static void AddModuleLStringResolver(
        this IServiceCollection services,
        IModule module)
    {
        var resolverType = GetModuleResolverType(module);
        if (resolverType is null) return;
        var getResource = GetJsonFileResourcesFn(module.LocalizationPath);
        var impl = (from type in resolverType.Assembly.GetTypes()
            where type.IsAssignableTo(resolverType) && !type.IsInterface
            select type).First();
        services.TryAddScoped(resolverType, sp =>
        {
            var getCulture = sp.GetRequiredService<GetCulture>();
            var resolver = new LStringResolver(getCulture, getResource);
            return Activator.CreateInstance(impl, resolver)!;
        });
    }

    private static GetCultureResource GetJsonFileResourcesFn(
        string localizationPath) =>
        culture =>
        {
            var jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                localizationPath,
                $"{culture}.json");
            if (!File.Exists(jsonPath)) return new Dictionary<string, string>();
            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
        };
}