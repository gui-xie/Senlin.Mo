using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo.Localization.Test;

public class LGeneratorTest
{
    [Fact]
    public Task GenerateWithDefaultFile()
    {
        var json = CreateAdditionalText(
            "l.json",
            "{\"name\":\"Name\",\"ageIs\":\"Age is {age}\"}"
        );
        const string srcText = @"
using Senlin.Mo.Localization.Abstractions;
namespace ProjectA {
    [LString]
    public enum Grade
    {
        Excellent,
        Good,
        Pass,
        Fail
    }
}
";
        var driver = GeneratorDriver(srcText, json);

        var results = driver.GetRunResult();

        return results.Verify();
    }

    private static GeneratorDriver GeneratorDriver(string srcText, params AdditionalText[] additionalTexts)
    {
        var compilation = CSharpCompilation.Create(
            "ProjectA",
            new[]
            {
                CSharpSyntaxTree.ParseText(srcText)
            },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(LStringAttribute).Assembly.Location),
            },
            options: null
        );

        ISourceGenerator[] generator = [new LGenerator().AsSourceGenerator()];

        var driver = CSharpGeneratorDriver.Create(generator, additionalTexts);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
}