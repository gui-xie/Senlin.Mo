using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo;

internal static class LocalizationExtensions
{
    public static IServiceCollection ConfigureLocalization(
        this IServiceCollection services,
        LocalizationOptions lOptions)
    {
        services.AddLocalization();
        services.AddRequestLocalization(options =>
        {
            options.SetDefaultCulture(lOptions.DefaultCulture);
            options.RequestCultureProviders.Insert(
                0,
                new CustomRequestCultureProvider(ctx => CustomProvider(ctx, lOptions)));
        });
        return services;
    }

    private static Task<ProviderCultureResult?> CustomProvider(HttpContext httpContext, LocalizationOptions options)
    {
        var request = httpContext.Request;
        // get from query string
        if (request.QueryString.HasValue && request.Query[options.CultureQueryStringKey].Count > 0)
        {
            var culture = request.Query[options.CultureQueryStringKey][0] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(culture))
            {
                return Task.FromResult(new ProviderCultureResult(culture))!;
            }
        }

        // get from header
        if (request.Headers.ContainsKey(options.CultureHeaderName))
        {
            var culture = request.Headers[options.CultureHeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(culture))
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

    internal static GetCulture GetCulture(this IServiceProvider sp, string defaultCulture)
    {
        var culture = sp.GetRequiredService<IHttpContextAccessor>()
                          .HttpContext!
                          .Features
                          .Get<IRequestCultureFeature>()
                          ?.RequestCulture.Culture.Name
                      ?? defaultCulture;
        return () => culture;
    }
}