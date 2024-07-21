using System.Reflection;
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

    private static Type? GetModuleResolverType(IEnumerable<Assembly> assemblies)
    {
        var resolverInterfaces =
            from t in assemblies.SelectMany(t => t.GetTypes())
            where t.IsAssignableTo(typeof(ILStringResolver)) && t.IsInterface
            select t;
        var resolver = resolverInterfaces.FirstOrDefault();
        return resolver;
    }

    public static void AddModuleLStringResolver(
        this IServiceCollection services,
        string localizationPath,
        IEnumerable<Assembly> assemblies)
    {
        var resolverType = GetModuleResolverType(assemblies);
        if (resolverType is null) return;
        var getResource = GetJsonFileResourcesFn(localizationPath);
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