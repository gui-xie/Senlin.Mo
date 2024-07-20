using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Senlin.Mo.Application.Abstractions.Decorators;
using Senlin.Mo.Application.Helpers;

namespace Senlin.Mo.Application
{
    /// <summary>
    /// Generate service 
    /// </summary>
    [Generator]
    public class ServiceGenerator : IIncrementalGenerator
    {
        private static readonly string DefaultUnitOfWorkAttribute = $"new {typeof(UnitOfWorkAttribute).FullName}()";

        private static readonly IReadOnlyCollection<string> DefaultServiceAttributes = [];

        private static readonly IReadOnlyCollection<string> DefaultCommandServiceAttributes =
            [DefaultUnitOfWorkAttribute];

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
                    IsServiceClassSyntax,
                    (ctx, _) => ToServiceInfo(ctx))
                .Where(s => s.ServiceCategory != ServiceCategory.None)
                .Combine(assemblyNameProvider);

            context.RegisterSourceOutput(provider, (ctx, p) =>
            {
                var s = p.Left;
                var serviceInterfaceFileName = $"I{s.ServiceName}.g.cs";
                ctx.AddSource(serviceInterfaceFileName, GenerateServiceInterfaceSource(s));
            });
        }

        private static bool IsServiceClassSyntax(SyntaxNode s, CancellationToken _)
        {
            return s is ClassDeclarationSyntax { BaseList.Types.Count: > 0 } c
                   && c.BaseList.Types.Any(t =>
                       t.Type is GenericNameSyntax genericNameSyntax
                       && (genericNameSyntax.Identifier.Text.Contains("IService")
                           || genericNameSyntax.Identifier.Text.Contains("ICommandService")));
        }

        private static string GetRequestTypeEndpointName(string requestTypeName)
        {
            var requestName = FirstCharToLower(requestTypeName.Split('.').Last());
            if (requestName.EndsWith("Dto", StringComparison.InvariantCultureIgnoreCase))
            {
                requestName = requestName.Substring(0, requestName.Length - 3);
            }

            return requestName;
        }

        private static string GenerateServiceInterfaceSource(ServiceInfo s)
        {
            var serviceName = s.ServiceName;
            var serviceInterfaceName = s.ServiceInterfaceInfo.Name;
            var requestTypeName = s.ServiceInterfaceInfo.RequestName;
            var requestName = GetRequestTypeEndpointName(requestTypeName);
            var responseTypeName = s.ServiceInterfaceInfo.ResponseName;
            var source = new StringBuilder();
            
            source.Append($$"""
    using Senlin.Mo.Application.Abstractions;
    using Senlin.Mo.Application.Abstractions.Decorators;
    namespace {{s.ServiceNamespace}}
    {
        public interface I{{serviceName}}
        {
            Task<{{responseTypeName}}> ExecuteAsync({{requestTypeName}} {{requestName}}, CancellationToken cancellationToken);
        
            public class {{serviceName}}Impl: I{{serviceName}}
            {
                private readonly {{serviceInterfaceName}} _service;
                
                public {{serviceName}}Impl({{serviceInterfaceName}} service)
                {
                    _service = service;
                }
                
                public Task<{{responseTypeName}}> ExecuteAsync({{requestTypeName}} {{requestName}}, CancellationToken cancellationToken)
                {
                    return _service.ExecuteAsync({{requestName}}, cancellationToken);
                }
            }
            
            public static ServiceRegistration Registration = new ServiceRegistration(
                typeof({{serviceInterfaceName}}),
                typeof({{serviceName}}),
                {{CreateServiceDecorators(s.ServiceDecorators, s.ServiceCategory)}}
            );
    {{CreateHandler(s)}}        
    {{CreateIdHandler(s)}}
        }
    }
    """);
            return source.ToString();
        }

        private static string CreateHandler(ServiceInfo s)
        {
            var serviceInterfaceName = s.ServiceInterfaceInfo.Name;
            var requestTypeName = s.ServiceInterfaceInfo.RequestName;
            var requestName = GetRequestTypeEndpointName(requestTypeName);
            return s.ServiceCategory == ServiceCategory.Command
                ? CreateCommandHandler(serviceInterfaceName, requestTypeName, requestName)
                : CreateQueryHandler(serviceInterfaceName, requestTypeName, s.IsRequestRecord, s.RequestProperties);
        }

        private static StringBuilder CreateIdHandler(ServiceInfo s)
        {
            var serviceInterfaceName = s.ServiceInterfaceInfo.Name;
            var requestTypeName = s.ServiceInterfaceInfo.RequestName;
            var sb = new StringBuilder();
            if(s.ServiceCategory != ServiceCategory.Command || !s.RequestProperties.Any(IsId)) return sb;
            var withoutIdProperties = s.RequestProperties.Where(p => !IsId(p)).ToArray();
            sb.AppendLine();
            var isSplitDto = withoutIdProperties.Length > 0;
            if (isSplitDto)
            {
                var splitDtoClassName = requestTypeName.Split('.').Last() + "_0";
                sb.Append(CreateSplitDto(s.RequestProperties, requestTypeName, s.IsRequestRecord, splitDtoClassName));
                sb.Append($$"""
                    public static Delegate IdHandler = (
                        {{serviceInterfaceName}} service,
                        string id,
                        {{splitDtoClassName}} dto,
                        CancellationToken cancellationToken) => {
                            return service.ExecuteAsync({{splitDtoClassName}}.ToDto(id, dto), cancellationToken);   
                        };
                """);
            }
            else
            {
                var createDto = s.IsRequestRecord ? "(id)": "{ Id = id}";
                sb.Append($$"""
                    public static Delegate IdHandler = (
                        {{serviceInterfaceName}} service,
                        string id,
                        CancellationToken cancellationToken) => {
                            return service.ExecuteAsync(
                                new {{requestTypeName}}{{createDto}}, 
                                cancellationToken);   
                        };
                """);                
            }
            sb.AppendLine();
            return sb;
        }
        
        private static string CreateCommandHandler(
            string serviceInterfaceName,
            string requestTypeName,
            string requestName) =>
            $$"""
                    public static Delegate Handler = (
                        {{serviceInterfaceName}} service,
                        {{requestTypeName}} {{requestName}},
                        CancellationToken cancellationToken) => {
                            return service.ExecuteAsync({{requestName}}, cancellationToken);   
                        };
            """;

        private static string CreateQueryHandler(
            string serviceInterfaceName,
            string requestTypeName,
            bool isRequestRecord,
            EquatableArray<TypeProperty> requestProperties) =>
            $$"""
                    public static Delegate Handler = (
                        {{serviceInterfaceName}} service,
                        {{CreateRequestPropertyFields(requestProperties)}}
                        CancellationToken cancellationToken) => {
                            return service.ExecuteAsync(
                                new {{requestTypeName}}
                                {{CreateQueryDto(requestProperties, isRequestRecord)}}, 
                                cancellationToken);   
                        };                   
            """;
        
        private static StringBuilder CreateQueryDto(EquatableArray<TypeProperty> properties, bool isRecord)
        {
            var sb = new StringBuilder();
            var flag = false;
            var createSymbol = "{}";
            Action<TypeProperty> assignProperty = p => sb.Append($"                        {p.Name} = {FirstCharToLower(p.Name)}");
            if (isRecord)
            {
                createSymbol = "()";
                assignProperty = p => sb.Append($"                        {FirstCharToLower(p.Name)}");
            }

            sb.AppendLine();
            sb.AppendLine($"            {createSymbol[0]}");
            foreach (var p in properties)
            {
                if (flag) sb.AppendLine(",");
                flag = true;
                assignProperty(p);
            }
            sb.AppendLine();
            sb.Append($"        {createSymbol[1]}");
            return sb;
        }

        private static StringBuilder CreateSplitDto(
            EquatableArray<TypeProperty> properties,
            string requestTypeName,
            bool isRequestRecord,
            string splitDtoClassName)
        {
            var sb = new StringBuilder();
            var flag = false;
            var createSymbol = "{}";
            Action<TypeProperty> assignProperty = p =>
            {
                if (IsId(p))
                {
                    sb.Append("            Id = id");
                    return;
                }
                sb.Append($"            {p.Name} = dto.{p.Name}");
            };
            if (isRequestRecord)
            {
                createSymbol = "()";
                assignProperty = p =>
                {
                    if (IsId(p))
                    {
                        sb.Append("            id");
                        return;
                    }
                    sb.Append($"        dto.{p.Name}");
                };
            }
            sb.AppendLine();
            sb.AppendLine($"    public class {splitDtoClassName}");
            sb.AppendLine("    {");
            foreach (var p in properties)
            {
                if(IsId(p)) continue;
                sb.AppendLine($"        public {p.TypeName} {p.Name} {{ get; set; }}");
            }
            sb.AppendLine();
            sb.AppendLine($"        public static {requestTypeName} ToDto(string id, {splitDtoClassName} dto)");
            sb.AppendLine("        {");
            sb.AppendLine($"            return new {requestTypeName}");
            sb.AppendLine($"            {createSymbol[0]}");
            foreach (var p in properties)
            {
                if (flag) sb.AppendLine(",");
                flag = true;
                assignProperty(p);
            }
            sb.AppendLine();
            sb.AppendLine($"            {createSymbol[1]};");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            return sb;
        }

        private static StringBuilder CreateRequestPropertyFields(EquatableArray<TypeProperty> properties)
        {
            var sb = new StringBuilder();
            foreach (var p in properties)
            {
                sb.AppendLine($"       {p.TypeName} {FirstCharToLower(p.Name)},");
            }
            return sb;
        }

        private static IEnumerable<string> GetDefaultServiceDecorators(ServiceCategory category) =>
            category == ServiceCategory.Command ? DefaultCommandServiceAttributes : DefaultServiceAttributes;

        private static StringBuilder CreateServiceDecorators(EquatableArray<string> serviceDecorators, ServiceCategory category)
        {
            var sb = new StringBuilder();
            sb.AppendLine("            new IServiceDecorator[]{");
            var decorators = GetServiceDecorators(serviceDecorators, category);
            foreach (var decorator in decorators)
            {
                sb.AppendLine($"                {decorator},");
            }
            sb.AppendLine("            }");
            return sb;
        }
        
        private static IEnumerable<string> GetServiceDecorators(
            EquatableArray<string> serviceDecorators, ServiceCategory category) =>
            serviceDecorators.Count == 0
                ? GetDefaultServiceDecorators(category)
                : serviceDecorators;

        private static string FirstCharToLower(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return char.ToLower(str[0]) + str.Substring(1);
        }

        private static bool IsId(TypeProperty t) => t.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase);

        private static TypeProperty[] GetRequestProperties(ITypeSymbol typeSymbol)
        {
            var properties =
                from member in typeSymbol.GetMembers()
                where member.Kind == SymbolKind.Property
                select (IPropertySymbol)member;
            return properties
                .Where(p => p.GetMethod is not null && p.SetMethod is not null)
                .Select(p => new TypeProperty(p))
                .ToArray();
        }

        private static ServiceInfo ToServiceInfo(GeneratorSyntaxContext ctx)
        {
            var s = (ClassDeclarationSyntax)ctx.Node;
            var interfaceSymbol = ctx.GetServiceInterfaceSymbol(s);
            if (interfaceSymbol is null || interfaceSymbol.TypeArguments.Length == 0)
            {
                return new ServiceInfo(
                    string.Empty,
                    string.Empty,
                    new ServiceInterfaceInfo(string.Empty, string.Empty, string.Empty),
                    new ServiceAttributeInfo(string.Empty, string.Empty, [], []),
                    ServiceCategory.None,
                    [],
                    false);
            }


            var isCommandService = interfaceSymbol.IsCommandService();
            var requestTypeSymbol = interfaceSymbol.TypeArguments[0];
            var responseTypeName = string.Empty;
            if (interfaceSymbol.TypeArguments.Length > 1)
            {
                var responseType = interfaceSymbol.TypeArguments[1];
                responseTypeName = responseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }

            var requestTypeName = requestTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (isCommandService)
            {
                responseTypeName = string.IsNullOrWhiteSpace(responseTypeName)
                    ? "global::Senlin.Mo.Domain.IResult"
                    : $"global::Senlin.Mo.Domain.IResult<{responseTypeName}>";
            }

            var requestProperties = GetRequestProperties(requestTypeSymbol);
            var isRequestRecord = requestTypeSymbol.IsRecord;
            var serviceInterfaceName = $"IService<{requestTypeName}, {responseTypeName}>";
            var serviceInterfaceInfo =
                new ServiceInterfaceInfo(serviceInterfaceName, requestTypeName, responseTypeName);

            return new ServiceInfo(
                s.Identifier.ToString(),
                GetNamespace(s),
                serviceInterfaceInfo,
                ctx.GetServiceAttributeInfo(s),
                isCommandService ? ServiceCategory.Command : ServiceCategory.Query,
                requestProperties,
                isRequestRecord);
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