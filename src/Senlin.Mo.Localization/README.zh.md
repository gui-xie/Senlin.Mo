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
* 检查生成文件L.g.cs、LResource.g.cs

* 演示
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
  * 使用生成文件（继承LResource或者使用JSON文件）
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