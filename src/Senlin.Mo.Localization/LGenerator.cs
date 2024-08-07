﻿using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Senlin.Mo.Localization;

/// <summary>
/// Localization config generator
/// </summary>
[Generator]
public class LGenerator : IIncrementalGenerator
{
    private static readonly AssemblyName ExecutingAssembly = Assembly.GetExecutingAssembly().GetName();
    private const string MoLocalizationFile = "build_property.MoLocalizationFile";
    private const string LStringAttributeName = "Senlin.Mo.Localization.Abstractions.LStringAttribute";
    private const string LStringKeyAttributeName = "LStringKey";
    private const string LStringAttributePrefix = "LString";

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        AddJsonLocalizationSource(context);
        AddEnumAttributeSource(context);
    }

    private static void AddJsonLocalizationSource(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(
            GetLocalizationFileProvider(context), (
                ctx, pair) =>
            {
                var file = pair.Left.Left;
                var assemblyName = pair.Left.Right;
                var lFileName = pair.Right;
                if (!file.Path.EndsWith(lFileName)) return;
                if (assemblyName is null) return;
                var jsonText = file.GetText()?.ToString() ?? string.Empty;
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
                if (dict is null) return;
                var infos = GetLStringInfos(dict);
                CreateLSource(ctx, assemblyName, infos);
                CreateLResourceSource(ctx, assemblyName, infos);
                CreateDynamicIl(ctx, assemblyName);
            });
    }

    private static void CreateDynamicIl(
        SourceProductionContext ctx,
        string assemblyName)
    {
        var source = new StringBuilder();
        source.AppendLine("#nullable enable");
        source.AppendLine("using Senlin.Mo.Localization.Abstractions;");
        source.AppendLine($"namespace {assemblyName}");
        source.AppendLine("{");
        source.AppendLine("    /// <summary>");
        source.AppendLine("    /// Localization string");
        source.AppendLine("    /// </summary>");
        source.AppendLine("    public interface IL : ILStringResolver");
        source.AppendLine("    {");
        source.AppendLine("        private class LImpl : IL");
        source.AppendLine("        {");
        source.AppendLine("            private readonly LStringResolver _lStringResolver;");
        source.AppendLine();
        source.AppendLine("            public LImpl(LStringResolver lStringResolver)");
        source.AppendLine("            {");
        source.AppendLine("                _lStringResolver = lStringResolver;");
        source.AppendLine("            }");
        source.AppendLine();
        source.AppendLine("            public string this[LString lString] => _lStringResolver[lString];");
        source.AppendLine("        }");
        source.AppendLine("    }");
        source.AppendLine("}");
        source.Append("#nullable restore");
        ctx.AddSource("IL.g.cs", source.ToString());
    }

    private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        var nameSpace = string.Empty;
        var syntaxParent = syntax.Parent;
        while (syntaxParent != null &&
               syntaxParent is not NamespaceDeclarationSyntax
               && syntaxParent is not FileScopedNamespaceDeclarationSyntax)
        {
            syntaxParent = syntaxParent.Parent;
        }

        if (syntaxParent is not BaseNamespaceDeclarationSyntax namespaceParent) return nameSpace;
        nameSpace = namespaceParent.Name.ToString();
        while (true)
        {
            if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
            {
                break;
            }

            nameSpace = $"{namespaceParent.Name}.{nameSpace}";
            namespaceParent = parent;
        }

        return nameSpace;
    }

    private sealed record EnumLStringInfo(INamedTypeSymbol AttributeSymbol, EnumDeclarationSyntax EnumSyntax)
    {
        public INamedTypeSymbol AttributeSymbol { get; } = AttributeSymbol;
        public EnumDeclarationSyntax EnumSyntax { get; } = EnumSyntax;
    }
    
    private static string GetSeparator(EnumDeclarationSyntax enumSyntax)
    {
        var separator = "_";
        foreach (var separatorArg in 
                 (from attr in enumSyntax.AttributeLists 
                     from attribute in attr.Attributes
                     where attribute.Name.ToString().StartsWith(LStringAttributePrefix) 
                     select attribute.ArgumentList?.Arguments.FirstOrDefault()))
        {
            if(separatorArg is null) continue;
            separator = separatorArg.Expression.ToString().Trim('"');
        }

        return separator;
    }
    
    private static void AddEnumAttributeSource(IncrementalGeneratorInitializationContext context)
    {
        var compilationContext = context
            .CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var attributeProviders = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                LStringAttributeName,
                (a, b) => a is EnumDeclarationSyntax,
                (ctx, _) => (ctx.TargetSymbol, TargetNode: (EnumDeclarationSyntax)ctx.TargetNode))
            .Where(x => x.TargetSymbol is INamedTypeSymbol)
            .Select((x, _) =>
                new EnumLStringInfo((INamedTypeSymbol)x.TargetSymbol, x.TargetNode))
            .Combine(compilationContext);

        context.RegisterSourceOutput(attributeProviders, (ctx, info) =>
        {
            var (attributeSymbol, enumSyntax) = info.Left;
            var assemblyName = info.Right ?? string.Empty;
            var enumName = attributeSymbol.Name;
            var enumFields = enumSyntax.Members;
            if (enumFields.Count == 0) return;
            var enumNamespace = GetNamespace(enumSyntax);
            var enumParameterName = enumName[0].ToString().ToLower() + enumName.Substring(1);
            var separator = GetSeparator(enumSyntax);
            var className = $"{enumName}Extensions";
            var source = new StringBuilder();
            source.AppendLine("#nullable enable");
            source.AppendLine("using Senlin.Mo.Localization.Abstractions;");
            if (!string.IsNullOrWhiteSpace(enumNamespace) && enumName != assemblyName)
            {
                source.AppendLine($"using {enumNamespace};");
            }

            source.AppendLine();
            source.AppendLine($"namespace {assemblyName}");
            source.AppendLine("{");
            source.AppendLine("    /// <summary>");
            source.AppendLine($"    /// {enumName} localization string extensions");
            source.AppendLine("    /// </summary>");
            source.AppendLine(
                $"    [System.CodeDom.Compiler.GeneratedCodeAttribute(\"{ExecutingAssembly.Name}\", \"{ExecutingAssembly.Version}\")]");
            source.AppendLine($"    public static partial class {className}");
            source.AppendLine("    {");
            source.AppendLine("        /// <summary>");
            source.AppendLine($"        /// Convert {enumName} to localization string");
            source.AppendLine("        /// </summary>");
            source.AppendLine($"        public static LString ToLString(this {enumName} {enumParameterName})");
            source.AppendLine("        {");
            source.AppendLine($"            return {enumParameterName} switch");
            source.AppendLine("            {");
            foreach (var enumField in enumFields)
            {
                var lKeyProperty = $"{enumName}{separator}{enumField.Identifier.Text}";
                foreach (var enumFieldAttr in enumField.AttributeLists)
                {
                    foreach (var fieldAttr in enumFieldAttr.Attributes)
                    {
                        if (fieldAttr.Name.ToString().EndsWith(LStringKeyAttributeName))
                        {
                            var attrValue = fieldAttr.ArgumentList?.Arguments[0]
                                .Expression.ToString().Trim('"');
                            if (attrValue is not null)
                            {
                                lKeyProperty = attrValue;
                            }
                        }
                    }
                }

                source.AppendLine(
                    $"                {enumName}.{enumField.Identifier.Text} => L.{lKeyProperty},");
            }

            source.AppendLine("                _ => LString.Empty");
            source.AppendLine("            };");
            source.AppendLine("        }");
            source.AppendLine("    }");
            source.AppendLine("}");
            source.Append("#nullable restore");

            ctx.AddSource($"{className}.g.cs", source.ToString());
        });
    }


    private static
        IncrementalValuesProvider<((AdditionalText Left, string? Right) Left, string Right)>
        GetLocalizationFileProvider(IncrementalGeneratorInitializationContext context) =>
        context
            .AdditionalTextsProvider
            .Combine(
                context
                    .CompilationProvider
                    .Select(static (c, _) => c.AssemblyName))
            .Combine(context.AnalyzerConfigOptionsProvider.Select(
                static (p, _) =>
                {
                    p.GlobalOptions.TryGetValue(MoLocalizationFile, out var file);
                    return file ?? "l.json";
                }));

    private static void CreateLResourceSource(
        SourceProductionContext ctx,
        string assemblyName,
        List<LStringInfo> infos)
    {
        var source = new StringBuilder();
        source.AppendLine("#nullable enable");
        source.AppendLine("using Senlin.Mo.Localization.Abstractions;");
        source.AppendLine("using System.Collections.Generic;");
        source.AppendLine($"namespace {assemblyName}");
        source.AppendLine("{");
        source.AppendLine("    /// <summary>");
        source.AppendLine("    /// Localization resource base class");
        source.AppendLine("    /// </summary>");
        source.AppendLine("    public abstract class LResource: ILResource");
        source.AppendLine("    {");
        source.AppendLine();
        source.AppendLine("        /// <summary>");
        source.AppendLine("        /// Culture");
        source.AppendLine("        /// </summary>");
        source.AppendLine("        public abstract string Culture { get; }");
        foreach (var info in infos)
        {
            source.AppendLine();
            source.AppendLine("        /// <summary>");
            source.AppendLine($"        /// {info.DefaultValue}");
            source.AppendLine("        /// </summary>");
            source.AppendLine($"        protected abstract string {info.KeyProperty} {{ get; }}");
        }

        source.AppendLine();
        source.AppendLine();
        source.AppendLine("        /// <summary>");
        source.AppendLine("        /// Get localization resource");
        source.AppendLine("        /// </summary>");
        source.AppendLine("        public Dictionary<string, string> GetResource() => new()");
        source.AppendLine("        {");
        foreach (var info in infos)
        {
            source.AppendLine($"            {{ \"{info.Key}\", {info.KeyProperty} }},");
        }

        source.AppendLine("        };");
        source.AppendLine("    }");
        source.AppendLine("}");
        source.AppendLine("#nullable restore");
        ctx.AddSource("LResource.g.cs", source.ToString());
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
        lResource.AppendLine("    /// <summary>");
        lResource.AppendLine("    /// Auto generated localization string");
        lResource.AppendLine("    /// </summary>");
        lResource.AppendLine(
            $"    [System.CodeDom.Compiler.GeneratedCodeAttribute(\"{ExecutingAssembly.Name}\", \"{ExecutingAssembly.Version}\")]");
        lResource.AppendLine("    public partial class L");
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
                lResource.AppendLine(
                    $"        public static LString {keyProperty} = new LString(\"{key}\", \"{defaultValue}\");");
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
            var escapeMatches = new List<string>();
            foreach (Match match in Regex.Matches(value, pattern))
            {
                if (match.Value.StartsWith("$"))
                {
                    escapeMatches.Add(match.Value);
                    continue;
                }
                if (tokens.Contains(match.Value)) continue;
                tokens.Add(match.Value);
            }

            value = escapeMatches.Aggregate(value,
                (current, escapeMatch) =>
                    current.Replace(escapeMatch, escapeMatch.Substring(1)));

            result.Add(new LStringInfo(key, value, keyProperty, tokens));
        }

        return result;
    }

    private class LStringInfo
    {
        public LStringInfo(string key, string defaultValue, string keyProperty, List<string> tokens)
        {
            Key = key;
            DefaultValue = defaultValue;
            KeyProperty = keyProperty;
            Tokens = tokens;
        }

        public string Key { get; }

        public string KeyProperty { get; }

        public string DefaultValue { get; }

        public List<string> Tokens { get; }
    }
}