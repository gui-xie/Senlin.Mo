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
                var interfaceTypeSyntax = (classSyntax.BaseList!.Types
                    .First(t => t.Type is GenericNameSyntax)
                    .Type as GenericNameSyntax)!;
              
                var interfaceName   = interfaceTypeSyntax.ToString();
                var className = classSyntax.Identifier.Text;
                var requestType = interfaceTypeSyntax.TypeArgumentList.Arguments[0];
                var responseTypeName =
                    interfaceTypeSyntax.TypeArgumentList.Arguments.Count == 1?
                        "Result":
                    $"Result<{interfaceTypeSyntax.TypeArgumentList.Arguments[1]}>";
                var isCommandService = false;
                if(interfaceName.Contains("Command"))
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
                var source = new StringBuilder();
                source.AppendLine("using Senlin.Mo.Application.Abstractions;");
                source.AppendLine("using Senlin.Mo.Domain;");
                source.AppendLine($"namespace {ns}");
                source.AppendLine("{");
                source.AppendLine($"    public static class {className}Extensions");
                source.AppendLine("    {");
                source.AppendLine($"        public static Delegate Handler = (");
                source.AppendLine($"                {requestTypeName} {requestName}, ");
                source.AppendLine($"                {interfaceName} service,");
                source.AppendLine("                CancellationToken cancellationToken) ");
                source.AppendLine("            => service.ExecuteAsync(");
                source.AppendLine($"                {requestName}, cancellationToken);");
                source.AppendLine();
                source.AppendLine($"        public static ServiceRegistration Registration = new ServiceRegistration(");
                source.AppendLine($"            typeof({interfaceName}),");
                source.AppendLine($"            typeof({className}),");
                source.AppendLine("            [");
                if (isCommandService)
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