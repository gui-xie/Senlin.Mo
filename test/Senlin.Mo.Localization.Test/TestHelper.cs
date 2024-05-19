global using static Senlin.Mo.Localization.Test.TestHelper;

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Senlin.Mo.Localization.Test;

public static class TestHelper
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }

    public static SettingsTask Verify(this GeneratorDriverRunResult results) =>
        Verifier.Verify(results)
            .UseDirectory("Snapshots");

    public static AdditionalText CreateAdditionalText(string path, string text) => 
        new AdditionalTextForTest(path, text);

    private class AdditionalTextForTest(string path, string text) : AdditionalText
    {
        public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
        {
            return SourceText.From(text, Encoding.UTF8);
        }

        public override string Path { get; } = path;
    }
}