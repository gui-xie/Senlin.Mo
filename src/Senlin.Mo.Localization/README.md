# Senlin.Mo.Localization

[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/gui-xie/Senlin.Mo/tree/master/src/Senlin.Mo.Localization)
[![GitHub](https://img.shields.io/github/license/gui-xie/Senlin.Mo?color=blue&label=License)](https://github.com/gui-xie/Senlin.Mo/blob/master/license.txt)

* reading in other languages: [English](README.md), [中文](README.zh.md).

## Introduction
Provide multi-language support similar to resx

## Quick Start

### Add NuGet package

```shell
dotnet add package Senlin.Mo.Localization
```

### Define JSON file
* Create l.json file in the project
```json
{
  "hello": "Hello",
  "sayHelloTo": "Hello {name}!"
}
```
* Modify the project configuration, set AdditionalFiles
```xml
<ItemGroup>
    <AdditionalFiles Include="l.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AdditionalFiles>
</ItemGroup>
```
* Check the generated file L.g.cs
```csharp
#nullable enable
using Senlin.Mo.Localization.Abstractions;
namespace Senlin.Mo
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Senlin.Mo.Localization", "1.0.1.0")]
    public static partial class L
    {
        // this can be used as default resource
        public static readonly Dictionary<string, string> LStringSource = new Dictionary<string, string>
        {
            {"hello", "Hello"},
            {"sayHelloTo", "Hello {name}!"},
        };

        /// <summary>
        /// Hello
        /// </summary>
        public static LString Hello = new LString("hello");

        /// <summary>
        /// Hello {name}!
        /// </summary>
        public static LString SayHelloTo(string name)
        {
            return new LString("sayHelloTo", new []{
                new KeyValuePair<string, string>("name", name),
            });
        }
    }
}
#nullable restore
```
* Directly add multi-language support in the code
```csharp
var resources = new Dictionary<string, Dictionary<string, string>>
{
    {
        "en",
        new Dictionary<string, string>
        {
            { "hello", "Hello" },
            { "sayHelloTo", "Hello {name}!" },
        }
    },
    {
        "zh",
        new Dictionary<string, string>
        {
            { "hello", "你好" },
            { "sayHelloTo", "你好 {name}!" },
        }
    }
};
GetCultureResource getCultureResource = culture => resources[culture];
var zhResolver = new LStringResolver(() => "zh", getCultureResource);
var hello = L.Hello.Resolve(zhResolver.Resolve);
Console.WriteLine(hello); // 你好

// Dependency injection
var services = new ServiceCollection();
services.AddScoped<GetCulture>(_ => () => "en");
services.AddSingleton(getCultureResource);
services.AddScoped<LStringResolver>();
using var sp = services.BuildServiceProvider();
using var s = sp.CreateScope();
var enResolver = sp.GetRequiredService<LStringResolver>();
var helloWorld = L.SayHelloTo("World").Resolve(enResolver.Resolve);
Console.WriteLine(helloWorld); // Hello World!
```
* Use JSON configuration multi-language
  * Create folder L
  * Add zh.json
   ```json
  {
    "hello": "你好",
    "sayHelloTo": "你好，{name}！"
  }
  ```
  * Configure the project
  ```xml
  <ItemGroup>
        <None Update="L\*.json">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>
  ```
  * Use in code
  ```csharp
  var zhResolver = new LStringResolver(() => "zh", GetJsonResource);
  var hello = L.SayHelloTo("世界").Resolve(zhResolver.Resolve);
  Console.WriteLine(hello); // 你好，世界！
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
  ```

[//]: # (* 默认使用l.json，如果需要修改，请按以下配置)
# Default use l.json file, if you need to modify, please follow the configuration
Configuration project MoLocalizationFile
```xml
<Project>
  <PropertyGroup>
      <MoLocalizationFile>l-customize.config</MoLocalizationFile>
  </PropertyGroup>
  <ItemGroup>
      <CompilerVisibleProperty Include="MoLocalizationFile" />
  </ItemGroup>
</Project>
```