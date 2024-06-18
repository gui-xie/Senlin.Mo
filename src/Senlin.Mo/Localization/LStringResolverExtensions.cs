using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo;

internal static class LStringResolverExtensions
{
    public static Type GetLStringResolverType(this IModule module)=> GetLStringResolverType(module.GetType());
    
    private static Type GetLStringResolverType(Type type) => typeof(ILStringResolver<>).MakeGenericType(type);
    
    public static void AddModuleLStringResolver(
        this IServiceCollection services, 
        IModule module,
        string localizationDirectory)
    {
        var getResource = GetJsonFileResourcesFn(localizationDirectory);
        var type = module.GetType();
        var lStringResolverType = GetLStringResolverType(type);
        services.TryAddScoped(lStringResolverType, sp =>
        {
            var getCulture = sp.GetRequiredService<GetCulture>();
            var resolver = new LStringResolver(getCulture, getResource);
            return Activator.CreateInstance(typeof(LStringResolver<>).MakeGenericType(type), resolver)!;
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
    
    private class LStringResolver<T>(ILStringResolver l): ILStringResolver<T>
    {
        public string this[LString lString] => l[lString];
    }
}