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
        var driver = GeneratorDriver("[assembly: Senlin.Mo.Localization.Abstractions.LocalizationConfig(Path = \"L\", Culture = \"zh\")]");
        
        var results = driver.GetRunResult();

        return  results.Verify();
    }

    private static GeneratorDriver GeneratorDriver(string srcText)
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
        var generator = new LGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
}
