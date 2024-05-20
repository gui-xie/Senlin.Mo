using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Senlin.Mo.Localization.Test;

public class LGeneratorTest
{
    [Fact]
    public Task GenerateWithoutDefaultJson()
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
    public Task MissingConfig()
    {
        var json = CreateAdditionalText(
            "/L/l.json", 
            "{\"namespace\":\"Test\",\"directory\":\"L\",\"name\":\"Name\",\"ageIs\":\"Age is {age}\"}"
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
            },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(LocalizableString).Assembly.Location)
            }
        );
        ISourceGenerator[] generator = [new LGenerator().AsSourceGenerator()];

        var driver = CSharpGeneratorDriver.Create(generator, additionalTexts);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
}