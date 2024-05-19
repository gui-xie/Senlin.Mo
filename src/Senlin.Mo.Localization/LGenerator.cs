using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Senlin.Mo.Localization;

/// <summary>
/// Localization config generator
/// </summary>
[Generator]
public class LGenerator : IIncrementalGenerator
{
    private const string ConfigName = "Senlin.Mo.Localization.Abstractions.LocalizationConfigAttribute";
    private const string ConfigShortName = "LocalizationConfig";
    
    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var configs = context.SyntaxProvider
            .ForAttributeWithMetadataName(
            ConfigName,
            predicate: static (_, _) => true,
            transform: static (ctx, _) =>
            {
                var s = (ISourceAssemblySymbol)ctx.TargetSymbol;
                var config = (ns: s.Name, path: "\"Localizations\"", culture: "\"en\"");
                var configAttr = ((CompilationUnitSyntax)ctx.TargetNode)
                    .AttributeLists
                    .SelectMany(x => x.Attributes)
                    .First(x => x.Name.ToString().Contains(ConfigShortName));
                if (configAttr.ArgumentList is null) return config;
                foreach (var argument in configAttr.ArgumentList.Arguments)
                {
                    var name = argument.NameEquals?.Name.ToString();
                    switch (name)
                    {
                        case "Path":
                            config.path = argument.Expression.ToString();
                            break;
                        case "Culture":
                            config.culture = argument.Expression.ToString();
                            break;
                    }
                }
                return config;
            }
        );
        context.RegisterSourceOutput(configs, (ctx, src) =>
        {
            ctx.AddSource("LocalizationConfig.cs",
                $$"""
                  namespace {{src.ns}};

                  public class LocalizationConfig
                  {
                      public const string Culture = {{src.culture}};
                      public const string Path = {{src.path}};
                  }
                  """
            );
        });
    }
}