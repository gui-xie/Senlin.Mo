using System.Text.Json;
using Senlin.Mo;

Console.WriteLine("Hello, Senlin.Mo!");
var memoryResolver = new LocalizationResolver("zh", GetMemoryResource);
var memoryResolverResult = L.AgeIs("3").Resolve(memoryResolver.Resolve);
Console.WriteLine(memoryResolverResult);

var jsonResolver = new LocalizationResolver("en", GetJsonResource);
var jsonResolverResult = L.AgeIs("8").Resolve(jsonResolver.Resolve);
Console.WriteLine(jsonResolverResult);

return;

static Dictionary<string, string> GetMemoryResource(string culture)
{
    var zh = new Dictionary<string, string>
    {
        {L.NameKey, "名称"},
        {L.AgeIsKey, "年龄为： {age}"}
    };
    var en = new Dictionary<string, string>
    {
        {nameof(L.NameKey), "Name"},
        {nameof(L.AgeIsKey), "Age is {age}"}
    };
    var resources = new Dictionary<string, Dictionary<string, string>>
    {
        {"zh", zh},
        {"en", en}
    };
    return resources[culture];
}

static Dictionary<string, string> GetJsonResource(string culture)
{
    var json = File.ReadAllText(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "L",
            $"{culture}.json"));
    return JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
}