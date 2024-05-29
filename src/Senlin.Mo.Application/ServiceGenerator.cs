using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo.Application
{
    /// <summary>
    /// Generate service 
    /// </summary>
    [Generator]
    public class ServiceGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Generate Service Registration and Handler Delegate
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context
                .SyntaxProvider
                .CreateSyntaxProvider(
                    (s, _) =>
                        s is ClassDeclarationSyntax z
                        && z.BaseList?.Types.Any(t =>
                            t.Type is GenericNameSyntax
                            {
                                Identifier.Text: "IService"
                            } or GenericNameSyntax
                            {
                                Identifier.Text: "ICommandService"
                            }) == true,
                    (ctx, c) => ctx);
            context.RegisterSourceOutput(provider, (ctx, syntaxContext) =>
            {
                var classSyntax = (ClassDeclarationSyntax)syntaxContext.Node;
                var symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
                if (symbol is null) return;

                var interfaceTypeSyntax = (classSyntax.BaseList!.Types
                    .First(t => t.Type is GenericNameSyntax)
                    .Type as GenericNameSyntax)!;

                var requestTypeSymbol = symbol.Interfaces[0].TypeArguments[0];
                var interfaceName = interfaceTypeSyntax.ToString();
                var className = classSyntax.Identifier.Text;

                var requestType = interfaceTypeSyntax.TypeArgumentList.Arguments[0];
                var responseTypeName =
                    interfaceTypeSyntax.TypeArgumentList.Arguments.Count == 1
                        ? "Result"
                        : $"Result<{interfaceTypeSyntax.TypeArgumentList.Arguments[1]}>";
                var isCommandService = false;
                if (interfaceName.Contains("Command"))
                {
                    isCommandService = true;
                    interfaceName = $"IService<{requestType}, {responseTypeName}>";
                }

                var requestTypeName = requestType.ToString();
                var requestName = FirstCharToLower(requestTypeName);
                if (requestName.EndsWith("Dto", StringComparison.InvariantCultureIgnoreCase))
                {
                    requestName = requestName.Substring(0, requestName.Length - 3);
                }

                var ns = GetNamespace(classSyntax);
                if (string.IsNullOrWhiteSpace(ns))
                {
                    ns = syntaxContext.SemanticModel.Compilation.AssemblyName;
                }

                var routeName = string.Empty;
                var methods = new List<string>();
                var isDisableUnitOfWork = !isCommandService;
                foreach (var attr in symbol.GetAttributes())
                {
                    if (attr.AttributeClass is null) continue;
                    if (attr.AttributeClass.Name == nameof(UnitOfWorkAttribute))
                    {
                        isDisableUnitOfWork = attr.ConstructorArguments.Length == 1
                                              && attr.ConstructorArguments[0]
                                                  .Value?.ToString()
                                                  .Equals("false", StringComparison.InvariantCultureIgnoreCase) == true;
                        continue;
                    }

                    if (attr.AttributeClass.Name == nameof(ServiceRouteAttribute))
                    {
                        routeName = attr.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
                        if (attr.ConstructorArguments.Length == 2)
                        {
                            methods = attr.ConstructorArguments[1].Values
                                .Select(v => v.Value?.ToString() ?? string.Empty)
                                .ToList();
                        }
                    }
                }


                var source = new StringBuilder();
                source.AppendLine("using Senlin.Mo.Application.Abstractions;");
                source.AppendLine("using Senlin.Mo.Domain;");
                source.AppendLine($"namespace {ns}");
                source.AppendLine("{");
                source.AppendLine($"    public static class {className}Extensions");
                source.AppendLine("    {");
                if (!string.IsNullOrWhiteSpace(routeName))
                {
                    source.AppendLine($"        public const string RouteName = \"{routeName}\";");
                    source.AppendLine();
                    if (methods.Count == 0)
                    {
                        methods.Add(isCommandService ? "POST" : "GET");
                    }

                    source.AppendLine(
                        $"        public static string[] Methods = new []{{\"{string.Join("\",\"", methods)}\"}};");
                    source.AppendLine();
                }

                source.AppendLine($"        public static Delegate Handler = (");
                List<(string PropertyType, string PropertyName)> properties = new();
                if (isCommandService)
                {
                    source.AppendLine($"                {requestTypeName} {requestName}, ");
                }
                else
                {
                    properties = GetQueryProperties(requestTypeSymbol);
                    foreach (var (propertyType, propertyName) in properties)
                    {
                        source.AppendLine($"                {propertyType} {FirstCharToLower(propertyName)}, ");
                    }
                }

                source.AppendLine($"                {interfaceName} service,");
                source.AppendLine("                CancellationToken cancellationToken) ");
                source.AppendLine("            => service.ExecuteAsync(");
                if (isCommandService)
                {
                    source.AppendLine($"                {requestName}, ");
                }
                else
                {
                    if (requestTypeSymbol.IsRecord)
                    {
                        source.Append($"                new {requestTypeName}(");
                        var flag = false;
                        foreach (var (_, propertyName) in properties)
                        {
                            if (flag)
                            {
                                source.Append(", ");
                            }

                            flag = true;
                            source.Append($"{FirstCharToLower(propertyName)}");
                        }
                        source.AppendLine("),");
                    }
                    else
                    {
                        source.AppendLine($"                new {requestTypeName}");
                        source.AppendLine("                {");
                        var flag = false;
                        foreach (var (_, propertyName) in properties)
                        {
                            if (flag)
                            {
                                source.Append(",");
                                source.AppendLine();
                            }

                            flag = true;
                            source.Append($"                  {propertyName} = {FirstCharToLower(propertyName)}");
                        }
                        source.AppendLine();
                        source.AppendLine("                },");

                    }

                }

                source.AppendLine("                cancellationToken);");
                source.AppendLine();
                source.AppendLine($"        public static ServiceRegistration Registration = new ServiceRegistration(");
                source.AppendLine($"            typeof({interfaceName}),");
                source.AppendLine($"            typeof({className}),");
                source.AppendLine("            [");

                if (isCommandService && !isDisableUnitOfWork)
                {
                    source.AppendLine("                typeof(UnitOfWorkDecorator<,,>),");
                }

                source.AppendLine("                typeof(LogDecorator<,>)");
                source.AppendLine("            ]");
                source.AppendLine("        );");
                source.AppendLine("    }");
                source.AppendLine("}");
                ctx.AddSource($"{ns}.{className}Extensions.cs", source.ToString());
            });
        }

        private static string FirstCharToLower(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return char.ToLower(str[0]) + str.Substring(1);
        }

        private static List<(string PropertyType, string PropertyName)> GetQueryProperties(ITypeSymbol typeSymbol)
        {
            var properties = from member in typeSymbol.GetMembers()
                where member.Kind == SymbolKind.Property
                select (IPropertySymbol)member;
            return properties
                .Where(p => p.GetMethod is not null && p.SetMethod is not null)
                .Select(p => (p.Type.ToDisplayString(), p.Name))
                .ToList();
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
    }
}