using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo.Test;

public class MoExtensionsTest
{
    [Fact]
    public void CultureTest()
    {
        var service = new ServiceCollection();
        service.AddScoped<GetCulture>(sp => ()=> "zh");
        service.ConfigureMo([]);
        
        var provider = service.BuildServiceProvider();
        using var s = provider.CreateScope();
        var getCulture = s.ServiceProvider.GetRequiredService<GetCulture>();
        
        var currentCulture = getCulture();
        
        currentCulture.Should().Be("zh");
    }
}