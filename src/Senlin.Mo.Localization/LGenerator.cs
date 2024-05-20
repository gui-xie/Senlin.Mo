using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using System.Text.Json;

namespace Senlin.Mo.Localization;

/// <summary>
/// Localization config generator
/// </summary>
[Generator]
public class LGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var localizationJsonRegex = new Regex(@"[\\/]l.json$");
        var jsonFiles = context
            .AdditionalTextsProvider
            .Where(t => localizationJsonRegex.IsMatch(t.Path));
        context.RegisterSourceOutput(jsonFiles, (ctx, file) =>
        {
            var jsonText = file.GetText()?.ToString() ?? string.Empty;
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            if (dict is null) return; 
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            const string namespaceKey = "namespace";
            const string directoryKey = "directory";
            if (!dict.TryGetValue(namespaceKey, out var namespaceValue)
                || !dict.TryGetValue(directoryKey, out var directoryValue))
            {
                DiagnosticDescriptor descriptor = new(
                    "LGenerator",
                    "Missing namespace or directory",
                    "Missing config in l.json",
                    "Localization",
                    DiagnosticSeverity.Error,
                    true
                );
                ctx.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None));
                return;
            }
            directoryValue = directoryValue.Replace("\\", @"\\");
            dict.Remove(namespaceKey);
            dict.Remove(directoryKey);
            
            var keyTokens = GetKeyTokens(dict);
            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using Senlin.Mo.Localization.Abstractions;");
            sb.AppendLine($"namespace {namespaceValue}");
            sb.AppendLine("{");
            sb.AppendLine($"    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"{assemblyName.Name}\", \"{assemblyName.Version}\")]");
            sb.AppendLine("    public static partial class L");
            sb.AppendLine("    {");
            sb.AppendLine($"        public const string Directory = \"{directoryValue}\";");
            foreach (var (key, tokens) in keyTokens)
            {
                var keyProperty = JsonKeyToPascalString(key);
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