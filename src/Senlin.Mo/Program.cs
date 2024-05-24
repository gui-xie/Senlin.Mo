using System.Text.Json;
using Senlin.Mo;
using Senlin.Mo.Localization.Abstractions;

var zhResolver = new LStringResolver(() => "zh", GetJsonResource);
var hello = L.SayHelloTo("世界").Resolve(zhResolver.Resolve);
Console.WriteLine(hello);
return;
  
static Dictionary<string, string> GetJsonResource(string culture)
{
    var json = File.ReadAllText(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "L",
            $"{culture}.json"));
    return JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
}