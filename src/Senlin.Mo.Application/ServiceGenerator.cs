using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Senlin.Mo.Application.Abstractions.Decorators.Log;
using Senlin.Mo.Application.Abstractions.Decorators.UnitOfWork;

namespace Senlin.Mo.Application
{
    /// <summary>
    /// Generate service 
    /// </summary>
    [Generator]
    public class ServiceGenerator : IIncrementalGenerator
    {
        private static readonly string DefaultLogAttribute = $"new {typeof(LogAttribute).FullName}()";
        private static readonly string DefaultUnitOfWorkAttribute = $"new {typeof(UnitOfWorkAttribute).FullName}()";

        private static readonly IReadOnlyCollection<string> DefaultServiceAttributes = [DefaultLogAttribute];
        private static readonly IReadOnlyCollection<string> DefaultCommandServiceAttributes = [DefaultUnitOfWorkAttribute, DefaultLogAttribute];
        
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
                var serviceExtensionsFileName = $"{s.ServiceName}Extensions.g.cs";
                ctx.AddSource(serviceExtensionsFileName, GenerateServiceExtensionsSource(s));
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

        private static string GenerateServiceExtensionsSource(ServiceInfo s)
        {
            return s.ServiceCategory == ServiceCategory.Command
                ? GenerateCommandServiceExtensionsSource(s)
                : GenerateQueryServiceExtensionsSource(s);
        }

        private static string GenerateQueryServiceExtensionsSource(ServiceInfo s)
        {
         
            var serviceName = s.ServiceName;
            var ns = s.ServiceNamespace;
            var serviceInterfaceName = s.ServiceInterfaceInfo.Name;
            var isContainsQuery = s.PatternMatchNames.Count > 0;
            var isCommand = s.ServiceCategory == ServiceCategory.Command;
            var requestName = s.ServiceInterfaceInfo.RequestName;
            var requestProperties = s.RequestProperties.ToArray();
            var matchNames = s.PatternMatchNames.ToArray();
            
            var source = new StringBuilder();
            source.AppendLine("using Senlin.Mo.Application.Abstractions;");
            source.AppendLine("using Senlin.Mo.Domain;");
            source.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            source.AppendLine($"namespace {ns}");
            source.AppendLine("{");
            source.AppendLine($"    public static class {serviceName}Extensions");
            source.AppendLine("    {");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                source.AppendLine($"        private const string Endpoint = \"{s.Endpoint}\";");
                source.AppendLine();
                var method = string.IsNullOrWhiteSpace(s.Method) ? "GET" : s.Method;
                source.AppendLine($"        private static string Method = \"{method}\";");
                source.AppendLine();
            }

            source.Append(CreateClassWithoutQueryParameters(s));
            source.AppendLine($"        public static Delegate Handler = (");
            AddParameters(source, isContainsQuery, isCommand, requestName, requestProperties, matchNames);
            source.AppendLine($"                {serviceInterfaceName} service,");
            source.AppendLine("                CancellationToken cancellationToken) ");
            source.AppendLine("            => service.ExecuteAsync(");
            AddRequestObject(source, s);
            source.AppendLine("                cancellationToken);");
            source.AppendLine();
            source.AppendLine($"        public static ServiceRegistration Registration = new ServiceRegistration(");
            source.AppendLine($"            typeof({serviceInterfaceName}),");
            source.AppendLine($"            typeof({serviceName}),");
            source.AppendLine("            [");
            IEnumerable<string> serviceDecorators = s.ServiceDecorators.Count == 0
                ? DefaultServiceAttributes
                : s.ServiceDecorators;
            foreach (var decorator in serviceDecorators)
            {
                source.AppendLine($"                {decorator},");
            }
            source.AppendLine("            ],");
            source.Append($"            ServiceLifetime.Transient");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                source.AppendLine(",");
                source.Append($"            new EndpointData(Endpoint, Handler, Method)");
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
            var serviceName = s.ServiceName;
            var ns = s.ServiceNamespace;
            var serviceInterfaceName = s.ServiceInterfaceInfo.Name;
            var isContainsQuery = s.PatternMatchNames.Count > 0;
            var isCommand = s.ServiceCategory == ServiceCategory.Command;
            var requestName = s.ServiceInterfaceInfo.RequestName;
            var requestProperties = s.RequestProperties.ToArray();
            var matchNames = s.PatternMatchNames.ToArray();

            var source = new StringBuilder();
            source.AppendLine("using Senlin.Mo.Application.Abstractions;");
            source.AppendLine("using Senlin.Mo.Domain;");
            source.AppendLine("using Microsoft.Extensions.DependencyInjection;");

            source.AppendLine($"namespace {ns}");
            source.AppendLine("{");
            source.AppendLine($"    public static class {serviceName}Extensions");
            source.AppendLine("    {");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                source.AppendLine($"        private const string Endpoint = \"{s.Endpoint}\";");
                source.AppendLine();
                var method = string.IsNullOrWhiteSpace(s.Method) ? "POST" : s.Method;
                source.AppendLine($"        private static string Method = \"{method}\";");
                source.AppendLine();
            }

            source.Append(CreateClassWithoutQueryParameters(s));
            source.AppendLine($"        public static Delegate Handler = (");
            AddParameters(source, isContainsQuery, isCommand, requestName, requestProperties, matchNames);
            source.AppendLine($"                {serviceInterfaceName} service,");
            source.AppendLine("                CancellationToken cancellationToken) ");
            source.AppendLine("            => service.ExecuteAsync(");
            AddRequestObject(source, s);
            source.AppendLine("                cancellationToken);");
            source.AppendLine();
            
            source.AppendLine($"        public static ServiceRegistration Registration = new ServiceRegistration(");
            source.AppendLine($"            typeof({serviceInterfaceName}),");
            source.AppendLine($"            typeof({serviceName}),");
            source.AppendLine("            [");
            IEnumerable<string> serviceDecorators = s.ServiceDecorators.Count == 0
                ? DefaultCommandServiceAttributes
                : s.ServiceDecorators;
            foreach (var decorator in serviceDecorators)
            {
                source.AppendLine($"                {decorator},");
            }
            source.AppendLine("            ],");

            source.Append($"            ServiceLifetime.Transient");
            if (!string.IsNullOrWhiteSpace(s.Endpoint))
            {
                source.AppendLine(",");
                source.Append($"            new EndpointData(Endpoint, Handler, Method)");
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
            if (interfaceSymbol is null)
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
                responseTypeName = responseType.ToDisplayString();
            }
            var requestTypeName = requestTypeSymbol.ToDisplayString();
            if (isCommandService)
            {
                responseTypeName = string.IsNullOrWhiteSpace(responseTypeName) 
                    ? "Result" 
                    : $"Result<{responseTypeName}>";
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

        private static bool IsContainsQuery(ServiceInfo s) => s.PatternMatchNames.Count > 0;

        private static string GetBodyClassName(string requestName) => $"{requestName.Split('.').Last()}0";

        private static IEnumerable<TypeProperty> GetBodyProperties(ServiceInfo s) =>
            from p in s.RequestProperties
            where !s.PatternMatchNames.Contains(p.Name, StringComparer.InvariantCultureIgnoreCase)
            select p;

        private static IEnumerable<TypeProperty> GetQueryProperties(
            TypeProperty[] requestProperties,
            string[] patternMatchNames
        ) =>
            from p in requestProperties
            where patternMatchNames.Contains(p.Name, StringComparer.InvariantCultureIgnoreCase)
            select p;

        private static string CreateClassWithoutQueryParameters(ServiceInfo s)
        {
            if (!IsContainsQuery(s)) return string.Empty;
            var properties = GetBodyProperties(s).ToList();
            if (properties.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            sb.AppendLine($"       private class {GetBodyClassName(s.ServiceInterfaceInfo.RequestName)}");
            sb.AppendLine("        {");
            foreach (var p in properties)
            {
                sb.AppendLine($"            public {p.TypeName} {p.Name} {{ get; set; }}");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
            return sb.ToString();
        }

        private static void AddParameters(
            StringBuilder source,
            bool isContainsQuery, 
            bool isCommand,
            string requestName,
            TypeProperty[] requestTypeProperties,
            string[] patternMatchNames)
        {
            if (isCommand && !isContainsQuery)
            {
                source.AppendLine($"                {requestName} {GetRequestTypeEndpointName(requestName)},");
                return;
            }

            if (isContainsQuery)
            {
                var queryProperties = GetQueryProperties(requestTypeProperties, patternMatchNames).ToList();
                foreach (var p in queryProperties)
                {
                    source.AppendLine($"                {p.TypeName} {FirstCharToLower(p.Name)},");
                }

                if (queryProperties.Count ==requestTypeProperties.Length) return;
                var className = GetBodyClassName(requestName);
                source.AppendLine($"                {className} {FirstCharToLower(className)},");
                return;
            }

            foreach (var p in requestTypeProperties)
            {
                source.AppendLine($"                {p.TypeName} {FirstCharToLower(p.Name)},");
            }
        }

        private static void AddRequestObject(StringBuilder source, ServiceInfo s)
        {
            var isContainsQuery = IsContainsQuery(s);
            if (s.ServiceCategory == ServiceCategory.Command && !isContainsQuery)
            {
                source.AppendLine($"                {GetRequestTypeEndpointName(s.ServiceInterfaceInfo.RequestName)},");
                return;
            }

            var parameters = s.RequestProperties.Select(p => new
            {
                IsQuery = true,
                p.TypeName,
                p.Name
            }).ToArray();
            if (isContainsQuery)
            {
                var queryProperties = GetQueryProperties(s.RequestProperties.ToArray(), s.PatternMatchNames.ToArray())
                    .Select(p => new
                    {
                        IsQuery = true,
                        p.TypeName,
                        p.Name
                    });
                var bodyProperties = GetBodyProperties(s)
                    .Select(p => new
                    {
                        IsQuery = false,
                        p.TypeName,
                        p.Name
                    });
                parameters = queryProperties.Concat(bodyProperties).ToArray();
            }

            var className = FirstCharToLower(GetBodyClassName(s.ServiceInterfaceInfo.RequestName));
            if (s.IsRequestRecord)
            {
                source.Append($"                new {s.ServiceInterfaceInfo.RequestName}(");
                var flag = false;
                foreach (var p in parameters)
                {
                    if (flag)
                    {
                        source.Append(",");
                    }

                    flag = true;
                    var propertyNameParameter = p.IsQuery
                        ? FirstCharToLower(p.Name)
                        : $"{className}.{p.Name}";
                    source.Append(propertyNameParameter);
                }

                source.AppendLine("),");
            }
            else
            {
                source.AppendLine($"                new {s.ServiceInterfaceInfo.RequestName}");
                source.AppendLine("                {");
                var flag = false;
                foreach (var p in parameters)
                {
                    if (flag)
                    {
                        source.Append(",");
                        source.AppendLine();
                    }

                    flag = true;
                    var propertyNameParameter = p.IsQuery
                        ? FirstCharToLower(p.Name)
                        : $"{className}.{p.Name}";
                    source.Append($"                    {p.Name} = {FirstCharToLower(propertyNameParameter)}");
                }

                source.AppendLine();
                source.AppendLine("                },");
            }
        }
    }
}