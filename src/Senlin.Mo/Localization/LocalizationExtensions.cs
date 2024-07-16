using Microsoft.AspNetCore.Builder;
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
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture(lOptions.SupportedCultures[0])
                .AddSupportedCultures(lOptions.SupportedCultures)
                .AddSupportedUICultures(lOptions.SupportedCultures);
        });
        return services;
    }

    internal static GetCulture GetCulture(this IServiceProvider sp, LocalizationOptions options)
    {
        var culture = sp.GetRequiredService<IHttpContextAccessor>()
                          .HttpContext?
                          .Features
                          .Get<IRequestCultureFeature>()
                          ?.RequestCulture.Culture.Name
                      ?? options.SupportedCultures[0];
        return () => culture;
    }
}