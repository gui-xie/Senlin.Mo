# Senlin.Mo.Localization

[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/gui-xie/Senlin.Mo/tree/master/src/Senlin.Mo.Localization)
[![GitHub](https://img.shields.io/github/license/gui-xie/Senlin.Mo?color=blue&label=License)](https://github.com/gui-xie/Senlin.Mo/blob/master/license.txt)

* reading in other languages: [English](README.md), [中文](README.zh.md).

## Introduction
Provide multi-language support, use source generator to create

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

[//]: # (* 检查生成文件L.g.cs、LResource.g.cs)

[//]: # ()
[//]: # (* 演示)

[//]: # (  * 创建文件夹L)

[//]: # (  * 添加zh.json)

[//]: # (  ```json)

[//]: # (  {)

[//]: # (    "hello": "你好",)

[//]: # (    "sayHelloTo": "你好，{name}！")

[//]: # (  })

[//]: # (  ```)

[//]: # (  * 配置项目)

[//]: # (  ```xml)

[//]: # (  <ItemGroup>)

[//]: # (        <None Update="L\*.json">)

[//]: # (            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>)

[//]: # (        </None>)

[//]: # (    </ItemGroup>)

[//]: # (  ```)

[//]: # (  * 使用生成文件（继承LResource或者使用JSON文件）)

[//]: # (  ```csharp)

[//]: # (  var zh = new ZhResource&#40;&#41;;)

[//]: # (  var ja = new JaResource&#40;&#41;;)

[//]: # (  var resourceProvider = new LResourceProvider&#40;zh, ja&#41;;)

[//]: # (  )
[//]: # (  var currentCulture = "zh";)

[//]: # (  GetCulture getCulture = &#40;&#41; => currentCulture;)

[//]: # (  GetCultureResource getCultureResource = resourceProvider.GetResource;)

[//]: # (  )
[//]: # (  var resolver = new LStringResolver&#40;getCulture, getCultureResource&#41;;)

[//]: # (  var zhHello = resolver.Resolve&#40;L.Hello&#41;;)

[//]: # (  Console.WriteLine&#40;zhHello&#41;;)

[//]: # (  )
[//]: # (  currentCulture = "ja";)

[//]: # (  var jaHello = resolver.Resolve&#40;L.Hello&#41;;)

[//]: # (  Console.WriteLine&#40;jaHello&#41;;)

[//]: # (  )
[//]: # (  Console.WriteLine&#40;resolver.Resolve&#40;L.SayHelloTo&#40;"世界"&#41;&#41;&#41;;)

[//]: # (  currentCulture = "unknown";)

[//]: # (  Console.WriteLine&#40;resolver.Resolve&#40;L.SayHelloTo&#40;"World"&#41;&#41;&#41;;)

[//]: # (  )
[//]: # (  public class JaResource : LResource)

[//]: # (  {)

[//]: # (      public override string Culture => "ja";)

[//]: # (  )
[//]: # (      protected override string Hello => "こんにちは";)

[//]: # (  )
[//]: # (      protected override string SayHelloTo => "こんにちは、{name}";)

[//]: # (  })

[//]: # (  )
[//]: # (  public class ZhResource : ILResource)

[//]: # (  {)

[//]: # (      public string Culture => "zh";)

[//]: # (  )
[//]: # (      public Dictionary<string, string> GetResource&#40;&#41;)

[//]: # (      {)

[//]: # (          var jsonPath = Path.Combine&#40;)

[//]: # (              Directory.GetCurrentDirectory&#40;&#41;,)

[//]: # (              "L",)

[//]: # (              $"{Culture}.json"&#41;;)

[//]: # (          var json = File.ReadAllText&#40;jsonPath&#41;;)

[//]: # (          return JsonSerializer.Deserialize<Dictionary<string, string>>&#40;json&#41;!;)

[//]: # (      })

[//]: # (  })

[//]: # (  ```)

[//]: # (* 默认使用l.json文件，如果需要修改，请按以下配置)

[//]: # (  配置项目的MoLocalizationFile)

[//]: # (```xml)

[//]: # (<Project>)

[//]: # (  <PropertyGroup>)

[//]: # (    <MoLocalizationFile>l-customize.config</MoLocalizationFile>)

[//]: # (  </PropertyGroup>)

[//]: # (  <ItemGroup>)

[//]: # (    <CompilerVisibleProperty Include="MoLocalizationFile" />)

[//]: # (  </ItemGroup>)

[//]: # (</Project>)

[//]: # (```)

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