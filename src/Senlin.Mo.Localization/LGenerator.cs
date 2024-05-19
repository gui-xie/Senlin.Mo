using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Senlin.Mo.Localization.Abstractions;
using System.Text.Json;

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
        var ns = "Senlin.Mo.Localization";
        var path = "Localization";
        var culture = "en";
        var configs = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ConfigName,
                predicate: static (_, _) => true,
                transform:  (ctx, _) =>
                {
                    ns = ctx.TargetSymbol.Name;
                    var configAttr = ((CompilationUnitSyntax)ctx.TargetNode)
                        .AttributeLists
                        .SelectMany(x => x.Attributes)
                        .First(x => x.Name.ToString().Contains(ConfigShortName));
                    if (configAttr.ArgumentList is null) return (ns, path, culture);
                    foreach (var argument in configAttr.ArgumentList.Arguments)
                    {
                        var name = argument.NameEquals?.Name.ToString();
                        switch (name)
                        {
                            case "Path":
                                path = argument.Expression.ToString().Trim('"');
                                break;
                            case "Culture":
                                culture = argument.Expression.ToString().Trim('"');
                                break;
                        }
                    }

                    return (ns, path, culture);
                }
            );
        context.RegisterSourceOutput(configs, (ctx, src) =>
        {
            ctx.AddSource("LConfig.g.cs",
                $$"""
                  namespace {{src.ns}};

                  public class LConfig
                  {
                      public const string Culture = "{{src.culture}}";
                      public const string Path = "{{src.path}}";
                  }
                  """
            );
        });

        var jsonFiles = context
            .AdditionalTextsProvider
            .Where(t=>t.Path.EndsWith(".json"));
        context.RegisterSourceOutput(jsonFiles, (ctx, file) =>
        {
            var regex = new Regex($@"{path}[\\/]{culture}.json$"); 
            if (!regex.IsMatch(file.Path)) return;
            var jsonText = file.GetText()?.ToString() ?? string.Empty;
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            if (dict is null) return; 
            var keyTokens = GetKeyTokens(dict);
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
          
            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using Senlin.Mo.Localization.Abstractions;");
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{assemblyName.Name}\", \"{assemblyName.Version}\")]");
            sb.AppendLine("    public static partial class L");
            sb.AppendLine("    {");
            foreach (var (key, tokens) in keyTokens)
            {
                var keyProperty = JsonKeyToPascalString(key);
                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// {dict[key]}");
                sb.AppendLine("        /// </summary>");
                if (tokens.Count == 0)
                {
                    sb.AppendLine($"        public static LocalizationString {keyProperty} = new LocalizationString(\"{key}\");");
                    continue;
                }
                sb.Append($"        public static LocalizationString {keyProperty}(");
                sb.Append(string.Join(", ", tokens.Select(t => $"string {t}")));
                sb.AppendLine($") => new LocalizationString(\"{key}\", new []{{ {string.Join(", ", tokens)} }});");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.Append("#nullable restore");
            
            ctx.AddSource("L.g.cs", sb.ToString());        
        });
        
    }

    private static string JsonKeyToPascalString(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str.Substring(1);
    }
    
    private static List<(string key, List<string> tokens)> GetKeyTokens(Dictionary<string, string> dict)
    {
        var result = new List<(string, List<string>)>();
        foreach (var keyValue in dict)
        {
            var key = keyValue.Key;
            var value = keyValue.Value;
            
            if(key is null || value is null) continue;
            
            var tokens = new List<string>();
            const string pattern = "(?<={)[^{}]+(?=})";
            foreach (Match match in Regex.Matches(value, pattern))
            {
                if(tokens.Contains(match.Value)) continue;
                tokens.Add(match.Value);
            }
            result.Add((key, tokens));
        }

        return result;
    }
}