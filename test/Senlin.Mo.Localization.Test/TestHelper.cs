using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

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
}