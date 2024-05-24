# Senlin.Mo.Localization

[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/gui-xie/Senlin.Mo)
[![GitHub](https://img.shields.io/github/license/gui-xie/Senlin.Mo?color=blue&label=License)](https://github.com/gui-xie/Senlin.Mo/blob/master/license.txt)

*用其他语言阅读: [English](README.md), [中文](README.zh.md).*

## 介绍
提供像resx支持多语言的功能 
定义JSON文件，基于Source Generator 生成

## 快速开始

### 添加NuGet包
```shell
dotnet add package Senlin.Mo.Localization
```
### 定义JSON文件
* 在项目中创建 l.json 文件
```json
{
  "hello": "Hello",
  "sayHelloTo": "Hello {name}!"
}
``` 
* 修改项目配置，设置AdditionalFiles
```xml
<ItemGroup>
    <AdditionalFiles Include="l.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AdditionalFiles>
</ItemGroup>
```
* 检查生成文件L.g.cs
```csharp
#nullable enable
using Senlin.Mo.Localization.Abstractions;
namespace Senlin.Mo
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Senlin.Mo.Localization", "0.0.1.0")]
    public static partial class L
    {
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
* 直接在代码中添加多语言支持
```csharp
// Directly use
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
Console.WriteLine(hello);

// Dependency injection
var services = new ServiceCollection();
services.AddScoped<GetCulture>(_ => () => "en");
services.AddSingleton(getCultureResource);
services.AddScoped<LStringResolver>();
using var sp = services.BuildServiceProvider();
using var s = sp.CreateScope();
var enResolver = sp.GetRequiredService<LStringResolver>();
var helloWorld = L.SayHelloTo("World");
Console.WriteLine(helloWorld);
```
* 使用JSON配置多语言
  * 创建文件夹L
  * 添加zh.json
  ```json
  {
    "hello": "你好",
    "sayHelloTo": "你好，{name}！"
  }
  ```
  * 配置项目
  ```xml
  <ItemGroup>
        <None Update="L\*.json">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>
  ```
  * 使用代码
  ```csharp
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
  ```
* 默认使用l.json文件，如果需要修改，请按以下配置
配置项目的MoLocalizationFile
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