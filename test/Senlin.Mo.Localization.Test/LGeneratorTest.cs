using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Senlin.Mo.Localization.Test;

public class LGeneratorTest
{
    [Fact]
    public Task GenerateWithoutJson()
    {
        var json = CreateAdditionalText(
            "/L/zh.json", 
            "{\"namespace\":\"Test\",\"name\":\"Name\",\"ageIs\":\"Age is {age}\"}"
        );
        var driver = GeneratorDriver(string.Empty, json);

        var results = driver.GetRunResult();

        return  results.Verify();
    }
    
    [Fact]
    public Task GenerateWithMoLocalizationJson()
    {
        var json = CreateAdditionalText(
            "mo-l.json", 
            "{\"name\":\"Name\",\"ageIs\":\"Age is {age}\"}"
        );
        var driver = GeneratorDriver(string.Empty, json);

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
            }
        );
        ISourceGenerator[] generator = [new LGenerator().AsSourceGenerator()];

        var driver = CSharpGeneratorDriver.Create(generator, additionalTexts);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
}