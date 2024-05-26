# Senlin.Mo.Localization

[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/gui-xie/Senlin.Mo/tree/master/src/Senlin.Mo.Localization)
[![GitHub](https://img.shields.io/github/license/gui-xie/Senlin.Mo?color=blue&label=License)](https://github.com/gui-xie/Senlin.Mo/blob/master/license.txt)

* reading in other languages: [English](README.md), [中文](README.zh.md).

## Introduction
Provide multi-language support, use source generator and json configuration file to generate resource files.

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

* Check the generated files L.g.cs, LResource.g.cs

* Example
  * Create folder L
  * Add zh.json
  ```json
  {
    "hello": "你好",
    "sayHelloTo": "你好，{name}！"
  }
  ```
    * Modify the project configuration
    ```xml
    <ItemGroup>
        <None Update="L\*.json">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>
    ```
  * Use the generated file (inherit LResource or use JSON file)
   ```csharp
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
  ```
  * Default use l.json file, if you need to modify, please configure the project's MoLocalizationFile
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