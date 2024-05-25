using System.Text.Json;
using Senlin.Mo;
using Senlin.Mo.Localization.Abstractions;

var zh = new ZhResource();
var ja = new JaResource();
var resourceProvider = new LResourceProvider(zh, ja);

var currentCulture = "zh";
GetCulture getCulture = () => currentCulture;
GetCultureResource getCultureResource = resourceProvider.GetResource;

var resolver = new LStringResolver(getCulture, getCultureResource);
var zhHello = resolver.Resolve(L.Hello);
Console.WriteLine(zhHello);

currentCulture = "ja";
var jaHello = resolver.Resolve(L.Hello);
Console.WriteLine(jaHello);

Console.WriteLine(resolver.Resolve(L.SayHelloTo("世界")));
currentCulture = "unknown";
Console.WriteLine(resolver.Resolve(L.SayHelloTo("World")));

public class JaResource : LResource
{
    public override string Culture => "ja";

    protected override string Hello => "こんにちは";

    protected override string SayHelloTo => "こんにちは、{name}";
}

public class ZhResource : ILResource
{
    public string Culture => "zh";

    public Dictionary<string, string> GetResource()
    {
        var jsonPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "L",
            $"{Culture}.json");
        var json = File.ReadAllText(jsonPath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
    }
}