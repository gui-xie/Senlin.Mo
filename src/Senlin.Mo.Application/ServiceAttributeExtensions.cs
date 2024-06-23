using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Application.Abstractions.Decorators;

namespace Senlin.Mo.Application;

internal static class ServiceAttributeExtensions
{
    private static readonly Regex PatternReg = new("{(.*)}");


    
    public static ServiceAttributeInfo GetServiceAttributeInfo(
        this GeneratorSyntaxContext ctx,
        ClassDeclarationSyntax s)
    {
        var endpoint = string.Empty;
        var method = string.Empty;
        var patternMatchNames = Array.Empty<string>();
        var serviceDecorators = new List<string>();
        
        var attributes =
            from attr in s.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes)
                let symbol = ctx.SemanticModel.GetSymbolInfo(attr).Symbol
                where symbol is IMethodSymbol
                select new
                {
                    Attribute = attr,
                    AttributeSymbol = (IMethodSymbol) symbol
                };
        foreach (var attr in attributes)
        {
            var attributeSyntax = attr.Attribute;
            var attributeSymbol = attr.AttributeSymbol;
            var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
            var fullName = attributeContainingTypeSymbol.ToDisplayString();
            var isServiceEndpointAttribute = fullName == typeof(ServiceEndpointAttribute).FullName;
            if (isServiceEndpointAttribute)
            {
                (endpoint, method, patternMatchNames) = GetEndpointInfos(attributeSyntax);
                continue;
            }
            var isServiceDecorator = attributeContainingTypeSymbol.AllInterfaces.Any(i =>
                i.ToDisplayString() == typeof(IServiceDecorator).FullName);
            if (isServiceDecorator){
                serviceDecorators.Insert(0, GetDecoratorSyntax(attributeSyntax, attributeSymbol));
            }
        }

        return new ServiceAttributeInfo(endpoint, method, patternMatchNames, serviceDecorators.ToArray());
    }
    
    private static (string endPoint, string method, string[] patternMatchNames) GetEndpointInfos(
        AttributeSyntax attributeSyntax)
    {
        var method = string.Empty;
        var endpoint = attributeSyntax.ArgumentList?.Arguments[0].Expression.ToString().Trim('"') ?? string.Empty;
        if (attributeSyntax.ArgumentList?.Arguments.Count == 2)
        {
            method = attributeSyntax
                .ArgumentList?.Arguments[1].Expression.ToString()
                .Trim('"').ToUpper()!;
        }
                
        var patternMatches = PatternReg.Matches(endpoint);
        var patternMatchNames = new string[patternMatches.Count];
        for (var i = 0; i < patternMatchNames.Length; i++)
        {
            patternMatchNames[i] = patternMatches[i].Groups[1].Value;
        }    
        return (endpoint, method, patternMatchNames);
    }
    
    private static string GetDecoratorSyntax(AttributeSyntax attributeSyntax, IMethodSymbol attributeSymbol)
    {
        var args = attributeSyntax.ArgumentList?.Arguments ?? [];
        var attributeCreateSyntax = new StringBuilder("new ");
        var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
        attributeCreateSyntax.Append(attributeContainingTypeSymbol.ToDisplayString());
        attributeCreateSyntax.Append("(");
        var idx = 0;
        foreach (var arg in args.Where(a => a.NameEquals == null))
        {
            if (idx > 0)
            {
                attributeCreateSyntax.Append(",");
            }
            attributeCreateSyntax.Append(arg);
            idx++;
        }
        attributeCreateSyntax.Append(")");
        idx = 0;
        foreach (var arg in args.Where(a => a.NameEquals != null))
        {
            attributeCreateSyntax.Append(idx == 0 ? "{" : ",");
            attributeCreateSyntax.Append(arg);
            idx++;
        }
        if (idx > 0)
        {
            attributeCreateSyntax.Append("}");
        }
        return attributeCreateSyntax.ToString();
    }
}