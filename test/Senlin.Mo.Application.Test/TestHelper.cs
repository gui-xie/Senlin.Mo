global using static Senlin.Mo.Application.Test.TestHelper;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Senlin.Mo.Application.Test;

public static class TestHelper
{
    [ModuleInitializer]
    public static void Init()
    {
        // VerifySourceGenerators.Initialize();
    }

    public static SettingsTask Verify(this GeneratorDriverRunResult results) =>
        Verifier.Verify(results)
            .UseDirectory("Snapshots");
}