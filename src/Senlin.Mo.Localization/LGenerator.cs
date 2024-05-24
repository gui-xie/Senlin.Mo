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
            
            var infos = GetLStringInfos(dict);
            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using Senlin.Mo.Localization.Abstractions;");
            sb.AppendLine($"namespace {assemblyName}");
            sb.AppendLine("{");
            sb.AppendLine($"    [System.CodeDom.Compiler.GeneratedCodeAttribute(\"{ExecutingAssembly.Name}\", \"{ExecutingAssembly.Version}\")]");
            sb.AppendLine("    public static partial class L");
            sb.AppendLine("    {");
            sb.AppendLine("        // this can be used as default resource");

            sb.AppendLine($"        public static readonly Dictionary<string, string> LStringSource = new Dictionary<string, string>");
            sb.AppendLine("        {");
            foreach (var info in infos)
            {
                sb.AppendLine($"            {{\"{info.Key}\", \"{info.DefaultValue}\"}},");
            }
            sb.AppendLine("        };");
            foreach (var info in infos)
            {
                var tokens = info.Tokens;
                var keyProperty = info.KeyProperty;
                var key = info.Key;
                sb.AppendLine();
                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// {dict[key]}");
                sb.AppendLine("        /// </summary>");
                if (tokens.Count == 0)
                {
                    sb.AppendLine($"        public static LString {keyProperty} = new LString(\"{key}\");");
                    continue;
                }
                sb.Append($"        public static LString {keyProperty}(");
                sb.Append(string.Join(", ", tokens.Select(t => $"string {t}")));
                sb.AppendLine(")");
                sb.AppendLine("        {");
                sb.AppendLine($"            return new LString(\"{key}\", new []{{");
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


    private static List<LStringInfo> GetLStringInfos(Dictionary<string, string> dict)
    {
        var result = new List<LStringInfo>();
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
            result.Add(new LStringInfo(key, value, keyProperty, tokens));
        }

        return result;
    }
    
    private class LStringInfo(
        string key, 
        string defaultValue, 
        string keyProperty, 
        List<string> tokens)
    {
        public string Key { get; } = key;

        public string KeyProperty { get; } = keyProperty;

        public string DefaultValue { get; } = defaultValue;

        public List<string> Tokens { get; } = tokens;
    }

}