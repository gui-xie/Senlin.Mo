using Microsoft.CodeAnalysis;

namespace Senlin.Mo.Application;

internal readonly record struct TypeProperty
{
    public TypeProperty(IPropertySymbol propertySymbol)
    {
        TypeName = propertySymbol.Type.ToDisplayString();
        Name = propertySymbol.Name;
    }

    public readonly string TypeName;
    public readonly string Name;
}