using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Senlin.Mo.Localization.Abstractions;

namespace Senlin.Mo.Localization.Test;

public class LGeneratorTest
{
    [Fact]
    public Task GenerateWithJsonFile()
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
        var compilation = CreateCompilation(srcText);
        ISourceGenerator[] generator = [new LGenerator().AsSourceGenerator()];
        var driver = CSharpGeneratorDriver.Create(generator, additionalTexts);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }

    private static CSharpCompilation CreateCompilation(string srcText)
    {
        SyntaxTree[] syntaxTrees =
        [
            CSharpSyntaxTree.ParseText(srcText)
        ];
        var compilation = CSharpCompilation.Create(
            "ProjectA", 
            syntaxTrees, 
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var references = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location));
        compilation = compilation.AddReferences(references);
        compilation = compilation.AddReferences(
            MetadataReference.CreateFromFile(typeof(LStringAttribute).Assembly.Location)
        );

        return compilation;
    }
}