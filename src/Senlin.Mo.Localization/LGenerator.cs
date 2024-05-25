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
            if(assemblyName is null) return;
            var jsonText = file.GetText()?.ToString() ?? string.Empty;
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            if (dict is null) return;

            var infos = GetLStringInfos(dict);
            CreateLSource(ctx, assemblyName, infos);
            CreateLResourceSource(ctx, assemblyName, infos);
        });
    }

    private static void CreateLResourceSource(
        SourceProductionContext ctx,
        string assemblyName, 
        List<LStringInfo> infos)
    {
        var lrSource = new StringBuilder();
        lrSource.AppendLine("#nullable enable");
        lrSource.AppendLine("using Senlin.Mo.Localization.Abstractions;");
        lrSource.AppendLine($"namespace {assemblyName}");
        lrSource.AppendLine("{");
        lrSource.AppendLine("    public abstract class LResource: ILResource");
        lrSource.AppendLine("    {");
        lrSource.AppendLine("        public abstract string Culture { get; }");
        foreach (var info in infos)
        {                
            lrSource.AppendLine();
            lrSource.AppendLine($"        protected abstract string {info.KeyProperty} {{ get; }}");
        }

        lrSource.AppendLine();
        lrSource.AppendLine("        public Dictionary<string, string> GetResource() => new()");
        lrSource.AppendLine("        {");
        foreach (var info in infos)
        {
            lrSource.AppendLine($"            {{ \"{info.Key}\", {info.KeyProperty} }},");
        }

        lrSource.AppendLine("        };");
        lrSource.AppendLine("    }");
        lrSource.AppendLine("}");
        lrSource.Append("#nullable restore");
        ctx.AddSource("LResource.g.cs", lrSource.ToString());
    }
    private static void CreateLSource(
        SourceProductionContext ctx,
        string assemblyName, 
        List<LStringInfo> infos)
    {
        var lResource = new StringBuilder();
            lResource.AppendLine("#nullable enable");
            lResource.AppendLine("using Senlin.Mo.Localization.Abstractions;");
            lResource.AppendLine($"namespace {assemblyName}");
            lResource.AppendLine("{");
            lResource.AppendLine(
                $"    [System.CodeDom.Compiler.GeneratedCodeAttribute(\"{ExecutingAssembly.Name}\", \"{ExecutingAssembly.Version}\")]");
            lResource.AppendLine("    public static partial class L");
            lResource.AppendLine("    {");
            var firstProperty = true;
            foreach (var info in infos)
            {
                var tokens = info.Tokens;
                var keyProperty = info.KeyProperty;
                var key = info.Key;
                var defaultValue = info.DefaultValue;
                if (!firstProperty)
                {
                    lResource.AppendLine();
                }
                firstProperty = false;
                lResource.AppendLine("        /// <summary>");
                lResource.AppendLine($"        /// {info.DefaultValue}");
                lResource.AppendLine("        /// </summary>");
                if (tokens.Count == 0)
                {
                    lResource.AppendLine($"        public static LString {keyProperty} = new LString(\"{key}\", \"{defaultValue}\");");
                    continue;
                }

                lResource.Append($"        public static LString {keyProperty}(");
                lResource.Append(string.Join(", ", tokens.Select(t => $"string {t}")));
                lResource.AppendLine(")");
                lResource.AppendLine("        {");
                lResource.AppendLine("            return new LString(");
                lResource.AppendLine($"                \"{key}\",");
                lResource.AppendLine($"                \"{defaultValue}\",");
                lResource.AppendLine("                new []");
                lResource.AppendLine("                {");
                foreach (var token in tokens)
                {
                    lResource.AppendLine($"                    new KeyValuePair<string, string>(\"{token}\", {token}),");
                }
                lResource.AppendLine("                }");
                lResource.AppendLine("            );");
                lResource.AppendLine("        }");
            }

            lResource.AppendLine("    }");
            lResource.AppendLine("}");
            lResource.Append("#nullable restore");

            ctx.AddSource("L.g.cs", lResource.ToString());
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

            if (key is null || value is null) continue;

            var keyProperty = JsonKeyToPascalString(key);

            var tokens = new List<string>();
            const string pattern = "(?<={)[^{}]+(?=})";
            foreach (Match match in Regex.Matches(value, pattern))
            {
                if (tokens.Contains(match.Value)) continue;
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