using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo.Application;

internal static class ServiceAttributeExtensions
{
    private static readonly Regex PatternReg = new("{(.*)}");

    public static ServiceAttributeInfo GetServiceAttributeInfo(this GeneratorSyntaxContext ctx, ClassDeclarationSyntax s)
    {
        var isUnitOfWork = true;
        var endpoint = string.Empty;
        var methods = new string[] { };
        var patternMatchNames = Array.Empty<string>();

        foreach (var attributeSyntax in s.AttributeLists.SelectMany(attributeListSyntax =>
                     attributeListSyntax.Attributes))
        {
            if (ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
            {
                continue;
            }

            var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
            var fullName = attributeContainingTypeSymbol.ToDisplayString();
            if (fullName == typeof(UnitOfWorkAttribute).FullName
                && attributeSymbol.TypeArguments.Length == 1
                && attributeSymbol.TypeArguments[0].ToDisplayString() == "false")
            {
                isUnitOfWork = true;
                continue;
            }

            if (fullName != typeof(ServiceEndpointAttribute).FullName) continue;
            endpoint = attributeSyntax.ArgumentList?.Arguments[0].Expression.ToString().Trim('"') ?? string.Empty;
            if (attributeSyntax.ArgumentList?.Arguments.Count == 2)
            {
                methods = attributeSyntax
                              .ArgumentList?.Arguments[1].Expression.ToString()
                              .Split(',')
                              .Select(t => t.Trim('"').ToUpper())
                              .ToArray()
                          ?? Array.Empty<string>();
            }

            var patternMatches = PatternReg.Matches(endpoint);

            patternMatchNames = new string[patternMatches.Count];
            for (var i = 0; i < patternMatchNames.Length; i++)
            {
                patternMatchNames[i] = patternMatches[i].Groups[1].Value;
            }
        }

        return new ServiceAttributeInfo(isUnitOfWork, endpoint, methods, patternMatchNames);
    }
}