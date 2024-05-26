using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Senlin.Mo;

internal static class LocalizationExtensions
{
    private const string CultureQueryStringKey = "culture";
    private const string CultureHeaderName = "Mo-Culture";
    private const string DefaultCulture = "en";
    
    public static IServiceCollection ConfigureLocalization(
        this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddRequestLocalization(options =>
        {
            options.SetDefaultCulture(DefaultCulture);
            options.RequestCultureProviders.Insert(
                0, 
                new CustomRequestCultureProvider(CustomProvider));
        });
        return services;
    }

    
    private static Task<ProviderCultureResult?> CustomProvider(HttpContext httpContext)
    {
        var request = httpContext.Request;
        // get from query string
        if (request.QueryString.HasValue && request.Query[CultureQueryStringKey].Count > 0)
        {
            var culture = request.Query[CultureQueryStringKey][0] ?? string.Empty;
            if(!string.IsNullOrWhiteSpace(culture))
            {
                return Task.FromResult(new ProviderCultureResult(culture))!;
            }
        }
        // get from header
        if (request.Headers.ContainsKey(CultureHeaderName))
        {
            var culture = request.Headers[CultureHeaderName].ToString();
            if(!string.IsNullOrWhiteSpace(culture))
            {
                return Task.FromResult(new ProviderCultureResult(culture))!;
            }
        }

        var languages = httpContext.Request.GetTypedHeaders().AcceptLanguage;
        var orderedLanguages = languages
            .Take(5)
            .OrderByDescending(x => x, StringWithQualityHeaderValueComparer.QualityComparer)
            .Select(x => x.Value)
            .ToList();
        if (orderedLanguages.Count <= 0)
        {
            return Task.FromResult(null as ProviderCultureResult);
        }
        return Task.FromResult(new ProviderCultureResult(orderedLanguages))!;
    }
    
    internal static string GetCulture(IHttpContextAccessor httpContextAccessor) =>
        httpContextAccessor.HttpContext!.Features.Get<IRequestCultureFeature>()
            ?.RequestCulture.Culture.Name 
        ?? DefaultCulture;
}