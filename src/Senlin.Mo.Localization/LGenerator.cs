using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Senlin.Mo.Localization;

/// <summary>
/// Localization config generator
/// </summary>
[Generator]
public class LGenerator : IIncrementalGenerator
{
    private static readonly AssemblyName ExecutingAssembly = Assembly.GetExecutingAssembly().GetName();

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var jsonFiles = 
            context
            .AdditionalTextsProvider
            .Combine(
                context
                    .CompilationProvider
                    .Select(static (c, _) => c.AssemblyName))
            .Combine(context.AnalyzerConfigOptionsProvider.Select(
                static (p, _) =>
                {
                    p.GlobalOptions.TryGetValue("build_property.MoLocalizationFile", out var file);
                    return file ?? "l.json";
                }));
        context.RegisterSourceOutput(jsonFiles, (
            ctx, pair) =>
        {
            var file = pair.Left.Left;
            var assemblyName = pair.Left.Right;
            var lFileName = pair.Right;
            if (!file.Path.EndsWith(lFileName)) return;
            var jsonText = file.GetText()?.ToString() ?? string.Empty;
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            if (dict is null) return; 
            
            var keyTokens = GetKeyTokens(dict);
            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using Senlin.Mo.Localization.Abstractions;");
            sb.AppendLine($"namespace {assemblyName}");
            sb.AppendLine("{");
            sb.AppendLine($"    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{ExecutingAssembly.Name}\", \"{ExecutingAssembly.Version}\")]");
            sb.AppendLine("    public static partial class L");
            sb.AppendLine("    {");
            foreach (var (key, keyProperty, _) in keyTokens)
            {
                sb.AppendLine($"        public static string {keyProperty}Key = \"{key}\";");
            }
            foreach (var (key, keyProperty, tokens) in keyTokens)
            {
                sb.AppendLine();
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
                sb.AppendLine(")");
                sb.AppendLine("        {");
                sb.AppendLine($"            return new LocalizationString(\"{key}\", new []{{");
                foreach (var token in tokens)
                {
                    sb.AppendLine($"                new KeyValuePair<string, string>(\"{token}\", {token}),");
                }
                sb.AppendLine("            });");
                sb.AppendLine("        }");
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
    
    private static List<(string key, string keyProperty, List<string> tokens)> GetKeyTokens(Dictionary<string, string> dict)
    {
        var result = new List<(string, string, List<string>)>();
        foreach (var keyValue in dict)
        {
            var key = keyValue.Key;
            var value = keyValue.Value;
            
            if(key is null || value is null) continue;
            
            var keyProperty = JsonKeyToPascalString(key);

            var tokens = new List<string>();
            const string pattern = "(?<={)[^{}]+(?=})";
            foreach (Match match in Regex.Matches(value, pattern))
            {
                if(tokens.Contains(match.Value)) continue;
                tokens.Add(match.Value);
            }
            result.Add((key, keyProperty, tokens));
        }

        return result;
    }
}