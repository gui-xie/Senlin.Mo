using Microsoft.CodeAnalysis;

namespace Senlin.Mo.Application;

internal readonly record struct TypeProperty
{
    public TypeProperty(IPropertySymbol propertySymbol)
    {
        TypeName = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        Name = propertySymbol.Name;
    }

    public readonly string TypeName;
    public readonly string Name;
}