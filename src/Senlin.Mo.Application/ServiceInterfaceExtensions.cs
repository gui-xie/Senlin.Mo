using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Senlin.Mo.Application;

internal static class ServiceInterfaceExtensions
{
    private const string CommandServiceTypeName = "Senlin.Mo.Application.Abstractions.ICommandService<";
    private const string ServiceTypeName = "Senlin.Mo.Application.Abstractions.IService<";
    private const string ServiceDecoratorType = "Senlin.Mo.Application.Abstractions.Decorators.IDecoratorService<";

    public static INamedTypeSymbol? GetServiceInterfaceSymbol(
        this GeneratorSyntaxContext ctx,
        ClassDeclarationSyntax s)
    {
        var baseTypes = s.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>();

        INamedTypeSymbol? tSymbol = null;
        foreach (var type in baseTypes)
        {
            if (ctx.SemanticModel.GetSymbolInfo(type.Type).Symbol is not INamedTypeSymbol
                {
                    IsGenericType: true
                } symbol) continue;
            var serviceGenericType = symbol.ToDisplayString();
            if (serviceGenericType.StartsWith(ServiceDecoratorType))
            {
                tSymbol = null;
                break;
            }
            if (serviceGenericType
                    .StartsWith(CommandServiceTypeName)
                || serviceGenericType
                    .StartsWith(ServiceTypeName))
            {
                tSymbol = symbol;
            }
        }
        return tSymbol;
    }
    
    public static bool IsCommandService(this INamedTypeSymbol symbol) => symbol.ToDisplayString().StartsWith(CommandServiceTypeName);
}