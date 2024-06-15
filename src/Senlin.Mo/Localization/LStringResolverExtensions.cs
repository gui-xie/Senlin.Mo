using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo;

internal static class LStringResolverExtensions
{
    public static Type GetLStringResolverType(this IModule module)
    {
        return typeof(LStringResolver<>).MakeGenericType(module.GetType());
    }
    
    public static void AddModuleLStringResolver(
        this IServiceCollection services, 
        IModule module,
        string localizationDirectory
        )
    {
        var getResource = GetJsonFileResourcesFn(localizationDirectory);
        var lStringResolverType = module.GetLStringResolverType();
        services.TryAddScoped(lStringResolverType, sp =>
        {
            var getCulture = sp.GetRequiredService<GetCulture>();
            return Activator.CreateInstance(
                typeof(LStringResolver), 
                getCulture,
                getResource,
                true)!;
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