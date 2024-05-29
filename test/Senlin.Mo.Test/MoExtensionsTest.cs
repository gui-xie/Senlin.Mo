using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo.Test;

public class MoExtensionsTest
{
    [Fact]
    public void CultureTest()
    {
        var service = new ServiceCollection();
        service.ConfigureMo(options =>
        {
            options.LocalizationOptions.DefaultCulture = "en";
        });
        var provider = service.BuildServiceProvider();
        using var s = provider.CreateScope();
        var culture = s.ServiceProvider.GetRequiredService<GetCulture>();
        
        Assert.Equal("en", culture());
    }
}