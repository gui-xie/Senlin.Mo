using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo.Localization.Test;

public class LGeneratorTest
{
    [Fact]
    public Task TestDefaultConfig()
    {
        var driver = GeneratorDriver("[assembly: Senlin.Mo.Localization.Abstractions.LocalizationConfig]");
        
        var results = driver.GetRunResult();
        
        return results.Verify();
    }
    
    [Fact]
    public Task TestCustomConfig()
    {
        const string srcText = "[assembly: Senlin.Mo.Localization.Abstractions.LocalizationConfig(Path = \"L\", Culture = \"zh\")]";
        var zhJson = CreateAdditionalText(
            "/L/zh.json", 
            "{\"name\":\"Name\",\"ageIs\":\"Age is {age}\"}"
            );
        var driver = GeneratorDriver(srcText, zhJson);

        var results = driver.GetRunResult();

        return  results.Verify();
    }

    private static GeneratorDriver GeneratorDriver(string srcText, params AdditionalText[] additionalTexts)
    {
        var compilation = CSharpCompilation.Create(
            "ProjectA",
            new[]
            {
                CSharpSyntaxTree.ParseText(
                    srcText, 
                    new CSharpParseOptions(LanguageVersion.CSharp12))
            },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(LocalizationConfigAttribute).Assembly.Location)
            }
        );
        ISourceGenerator[] generator = [new LGenerator().AsSourceGenerator()];

        var driver = CSharpGeneratorDriver.Create(generator, additionalTexts);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
}
