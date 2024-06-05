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
            var assemblyNameProvider = context.CompilationProvider
                .Select((c, _) => c.AssemblyName);
            var provider = context
                .SyntaxProvider
                .CreateSyntaxProvider(
                    IsClassSyntax,
                    (ctx, c) => ToServiceInfo(ctx))
                .Where(s=> s.ServiceSyntax is not null)
                .Combine(assemblyNameProvider);

            context.RegisterSourceOutput(provider, (ctx, p) =>
            {
                var s = p.Left!;
                var serviceExtensionsFileName = $"{s.ClassSyntax.Identifier}Extensions.g.cs";
                ctx.AddSource(serviceExtensionsFileName, GenerateServiceExtensionsSource(s));
            });
        }

        private static bool IsClassSyntax(SyntaxNode s, CancellationToken _) =>
            s is ClassDeclarationSyntax z;

        private sealed record ServiceInfo(
            ClassDeclarationSyntax ClassSyntax,
            GenericNameSyntax? ServiceSyntax,
            bool IsCommandService,
            bool IsEnableUnitOfWork,
            string Endpoint,
            string[] Methods,
            INamedTypeSymbol? ServiceRequestSymbol)
        {
            public ClassDeclarationSyntax ClassSyntax { get; } = ClassSyntax;

            public GenericNameSyntax? ServiceSyntax { get; } = ServiceSyntax;
            

            public bool IsCommandService { get; } = IsCommandService;
            
            public bool IsEnableUnitOfWork { get; } = IsEnableUnitOfWork;

            public string Endpoint { get; } = Endpoint;

            public string[] Methods { get; } = Methods;
            
            public INamedTypeSymbol? ServiceRequestSymbol { get; } = ServiceRequestSymbol;
        }

        private static string GetRequestTypeEndpointName(string requestTypeName)
        {
            var requestName = FirstCharToLower(requestTypeName);
            if (requestName.EndsWith("Dto", StringComparison.InvariantCultureIgnoreCase))
            {
                requestName = requestName.Substring(0, requestName.Length - 3);
            }

            return requestName;
        }

        private static string GenerateServiceExtensionsSource(ServiceInfo s) =>
            s.IsCommandService
                ? GenerateCommandServiceExtensionsSource(s) 
                : GenerateQueryServiceExtensionsSource(s);

        private static string GenerateQueryServiceExtensionsSource(ServiceInfo s)
        {
            var service = s.ServiceSyntax!;
            var className = s.ClassSyntax.Identifier.ToString();
            var ns = GetNamespace(s.ClassSyntax);
            var serviceName = service.ToString();
            var serviceRequestSymbol = s.ServiceRequestSymbol!;
            var properties = GetQueryProperties(serviceRequestSymbol);
            
            var source = new StringBuilder();
            source.AppendLine("using Senlin.Mo.Application.Abstractions;");
            source.AppendLine("using Senlin.Mo.Domain;");
            source.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            source.AppendLine($"namespace {ns}");
            source.AppendLine("{");
            source.AppendLine($"    public static class {className}Extensions");
            source.AppendLine("    {");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                source.AppendLine($"        private const string Endpoint = \"{s.Endpoint}\";");
                source.AppendLine();
                var methods = s.Methods;
                if (methods.Length == 0)
                {
                    methods = new[] { "GET" };
                }
                source.AppendLine(
                    $"        private static string[] Methods = new []{{\"{string.Join("\",\"", methods)}\"}};");
                source.AppendLine();
            }
            source.AppendLine($"        public static Delegate Handler = (");
            AddQueryParameters(source, properties);
            source.AppendLine($"                {serviceName} service,");
            source.AppendLine("                CancellationToken cancellationToken) ");
            source.AppendLine("            => service.ExecuteAsync(");
            AddRequestObject(source, properties, serviceRequestSymbol);
            source.AppendLine("                cancellationToken);");
            source.AppendLine();
            source.AppendLine($"        public static ServiceRegistration Registration = new ServiceRegistration(");
            source.AppendLine($"            typeof({serviceName}),");
            source.AppendLine($"            typeof({className}),");
            source.AppendLine("            [");
            source.AppendLine("                typeof(LogDecorator<,>)");
            source.AppendLine("            ],");
            source.Append($"            ServiceLifetime.Transient");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                source.AppendLine(",");
                source.Append($"            new EndpointData(Endpoint, Handler, Methods)");
            }
            source.AppendLine();
            source.AppendLine("        );");
            source.AppendLine("    }");
            source.AppendLine("}");
            return source.ToString();
        }
        
        private static string GenerateCommandServiceExtensionsSource(
            ServiceInfo s)
        {
            var service = s.ServiceSyntax!;
            var className = s.ClassSyntax.Identifier.ToString();
            var requestType = service.TypeArgumentList.Arguments[0];
            var ns = GetNamespace(s.ClassSyntax);
            var responseType = service.TypeArgumentList.Arguments.Count == 1
                ? "Result"
                : $"Result<{service.TypeArgumentList.Arguments[1]}>";
            var serviceName = $"IService<{requestType}, {responseType}>";
            var requestTypeName = service.TypeArgumentList.Arguments[0].ToString();
            var endpointRequestName = GetRequestTypeEndpointName(requestTypeName);
            var requestProperties = s.ServiceRequestSymbol != null
                ? GetQueryProperties(s.ServiceRequestSymbol)
                : new List<(string, string)>();
            
            var source = new StringBuilder();
            source.AppendLine("using Senlin.Mo.Application.Abstractions;");
            source.AppendLine("using Senlin.Mo.Domain;");
            source.AppendLine("using Microsoft.Extensions.DependencyInjection;");

            source.AppendLine($"namespace {ns}");
            source.AppendLine("{");
            source.AppendLine($"    public static class {className}Extensions");
            source.AppendLine("    {");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                var methods = s.Methods;
                if (methods.Length == 0)
                {
                    methods = new[] { "POST" };
                }
                source.AppendLine($"        private const string Endpoint = \"{s.Endpoint}\";");
                source.AppendLine();
                source.AppendLine(
                    $"        private static string[] Methods = new []{{\"{string.Join("\",\"", methods)}\"}};");
                source.AppendLine();
            }
            source.AppendLine($"        public static Delegate Handler = (");
            if (requestProperties.Any())
            {
                AddQueryParameters(source, requestProperties);
            }
            else
            {
                source.AppendLine($"                {requestTypeName} {endpointRequestName}, ");
            }
            source.AppendLine($"                {serviceName} service,");
            source.AppendLine("                CancellationToken cancellationToken) ");
            source.AppendLine("            => service.ExecuteAsync(");
            if (requestProperties.Any())
            {
                AddRequestObject(source, requestProperties, s.ServiceRequestSymbol!);
            }
            else
            {
                source.AppendLine($"                {endpointRequestName},");
            }
            source.AppendLine("                cancellationToken);");
            source.AppendLine();
            source.AppendLine($"        public static ServiceRegistration Registration = new ServiceRegistration(");
            source.AppendLine($"            typeof({serviceName}),");
            source.AppendLine($"            typeof({className}),");
            source.AppendLine("            [");
            if (s.IsEnableUnitOfWork)
            {
                source.AppendLine("                typeof(UnitOfWorkDecorator<,,>),");
            }
            source.AppendLine("                typeof(LogDecorator<,>)");
            source.AppendLine("            ],");

            source.Append($"            ServiceLifetime.Transient");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                source.AppendLine(",");
                source.Append($"            new EndpointData(Endpoint, Handler, Methods)");
            }

            source.AppendLine();
            source.AppendLine("        );");
            source.AppendLine("    }");
            source.AppendLine("}");
            return source.ToString();
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

        private static ServiceInfo ToServiceInfo(GeneratorSyntaxContext ctx)
        {
            var s = (ClassDeclarationSyntax)ctx.Node;
            GenericNameSyntax? serviceType = null;
            var isCommandService = false;
            var isUnitOfWork = true;
            var endpoint = string.Empty;
            var methods = new string[]{};
            foreach (var attributeSyntax in s.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
            {
                if (ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol
                    is not IMethodSymbol attributeSymbol)
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
                        .Select(t=>t.Trim('"').ToUpper())
                        .ToArray()
                              ?? Array.Empty<string>();
                }
            }
            var baseTypes = s.BaseList?.Types ?? Enumerable.Empty<BaseTypeSyntax>();
            INamedTypeSymbol? serviceRequestSymbol = null;
            foreach (var type in baseTypes)
            {
                if(ctx.SemanticModel.GetSymbolInfo(type.Type).Symbol is not INamedTypeSymbol { IsGenericType: true } symbol) continue;
                var serviceGenericType = symbol.ToDisplayString();
                const string commandServiceTypeName = "Senlin.Mo.Application.Abstractions.ICommandService<";
                const string serviceTypeName = "Senlin.Mo.Application.Abstractions.IService<";
                var queryReuqestFlag = false;
                if(serviceGenericType.StartsWith(commandServiceTypeName))
                {
                    isCommandService = true;
                    if (methods.Contains("DELETE", StringComparer.InvariantCulture))
                    {
                        queryReuqestFlag = true;
                    }
                }
                else if (serviceGenericType.StartsWith(serviceTypeName))
                {
                    queryReuqestFlag = true;
                }
                serviceType = (GenericNameSyntax)type.Type;
                if(!queryReuqestFlag) continue;

                serviceRequestSymbol =
                    ctx.SemanticModel.GetSymbolInfo(serviceType.TypeArgumentList.Arguments[0]).Symbol as INamedTypeSymbol;
            }

            return new ServiceInfo(
                s, 
                serviceType,
                isCommandService,
                isUnitOfWork,
                endpoint,
                methods,
                serviceRequestSymbol);
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

        private static void AddQueryParameters(StringBuilder source, List<(string Type, string Name)> properties)
        {
            foreach (var (t, n)  in properties)
            {
                source.AppendLine($"                {t} {FirstCharToLower(n)},");
            }
        }
        
        private static void AddRequestObject(StringBuilder source, List<(string Type, string Name)> properties, INamedTypeSymbol serviceRequestSymbol)
        {
            if (serviceRequestSymbol.IsRecord)
            {
                source.Append($"                new {serviceRequestSymbol}(");
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
                source.AppendLine($"                new {serviceRequestSymbol}");
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
                    source.Append($"                    {propertyName} = {FirstCharToLower(propertyName)}");
                }
                source.AppendLine();
                source.AppendLine("                },");
            }
        }
    }
}