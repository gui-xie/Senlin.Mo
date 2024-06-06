using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly Regex PatternReg = new("{(.*)}");
        
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
                    (ctx, _) => ToServiceInfo(ctx))
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
            s is ClassDeclarationSyntax;

        private sealed record ServiceInfo(
            ClassDeclarationSyntax ClassSyntax,
            GenericNameSyntax? ServiceSyntax,
            bool IsCommandService,
            bool IsEnableUnitOfWork,
            string Endpoint,
            string[] Methods,
            string RequestName,
            List<(string PropertyType, string Name)> RequestProperties,
                bool IsRequestRecord,
                    string[] PatternMatchNames)
        {
            public ClassDeclarationSyntax ClassSyntax { get; } = ClassSyntax;

            public GenericNameSyntax? ServiceSyntax { get; } = ServiceSyntax;
            

            public bool IsCommandService { get; } = IsCommandService;
            
            public bool IsEnableUnitOfWork { get; } = IsEnableUnitOfWork;

            public string Endpoint { get; } = Endpoint;

            public string[] Methods { get; } = Methods;
            
            public string RequestName { get; } = RequestName;
            
            public List<(string PropertyType, string Name)> RequestProperties { get; } = RequestProperties;
            
            public bool IsRequestRecord { get; } = IsRequestRecord;
            
            public string[] PatternMatchNames { get; } = PatternMatchNames;
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

            source.Append(CreateClassWithoutQueryParameters(s));
            source.AppendLine($"        public static Delegate Handler = (");
            AddParameters(source, s);
            source.AppendLine($"                {serviceName} service,");
            source.AppendLine("                CancellationToken cancellationToken) ");
            source.AppendLine("            => service.ExecuteAsync(");
            AddRequestObject(source, s);
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
            source.Append(CreateClassWithoutQueryParameters(s));
            source.AppendLine($"        public static Delegate Handler = (");
            AddParameters(source, s);
            source.AppendLine($"                {serviceName} service,");
            source.AppendLine("                CancellationToken cancellationToken) ");
            source.AppendLine("            => service.ExecuteAsync(");
            AddRequestObject(source, s);
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

        private static List<(string PropertyType, string Name)> GetQueryProperties(ITypeSymbol typeSymbol)
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
            var patternMatchNames = Array.Empty<string>();
            List<(string, string)> requestProperties = new();
            var isRequestRecord = false;
            
            foreach (var type in baseTypes)
            {
                if(ctx.SemanticModel.GetSymbolInfo(type.Type).Symbol is not INamedTypeSymbol { IsGenericType: true } symbol) continue;
                var serviceGenericType = symbol.ToDisplayString();
                const string commandServiceTypeName = "Senlin.Mo.Application.Abstractions.ICommandService<";
                var patternMatches = PatternReg.Matches(endpoint);
                patternMatchNames = new string[patternMatches.Count];
                for (var i = 0; i < patternMatchNames.Length; i++)
                {
                    patternMatchNames[i] = patternMatches[i].Groups[1].Value;
                }
                
                if(serviceGenericType.StartsWith(commandServiceTypeName))
                {
                    isCommandService = true;
                }
                serviceType = (GenericNameSyntax)type.Type;

                serviceRequestSymbol =
                    ctx.SemanticModel.GetSymbolInfo(serviceType.TypeArgumentList.Arguments[0]).Symbol as INamedTypeSymbol;
                
                if(serviceRequestSymbol is null) continue;
                isRequestRecord = serviceRequestSymbol.IsRecord;
                requestProperties = GetQueryProperties(serviceRequestSymbol);
            }

            return new ServiceInfo(
                s, 
                serviceType,
                isCommandService,
                isUnitOfWork,
                endpoint,
                methods,
                serviceRequestSymbol?.Name ?? string.Empty,
                requestProperties,
                isRequestRecord,
                patternMatchNames);
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

        private static bool IsContainsQuery(ServiceInfo s) => s.PatternMatchNames.Length > 0;

        private static string GetBodyClassName(ServiceInfo s) => $"{s.RequestName}0";

        private static IEnumerable<(string PropertyType, string Name)> GetBodyProperties(ServiceInfo s) =>
            from p in s.RequestProperties
            where !s.PatternMatchNames.Contains(p.Name, StringComparer.InvariantCultureIgnoreCase)
            select (p.PropertyType, p.Name);

        private static IEnumerable<(string PropertyType, string Name)> GetQueryProperties(ServiceInfo s) =>
            from p in s.RequestProperties
            where s.PatternMatchNames.Contains(p.Name, StringComparer.InvariantCultureIgnoreCase)
            select (p.PropertyType, p.Name);

        private static string CreateClassWithoutQueryParameters(ServiceInfo s)
        {
            if (!IsContainsQuery(s)) return string.Empty;
            var properties = GetBodyProperties(s).ToList();
            if (properties.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            sb.AppendLine($"       private class {GetBodyClassName(s)}");
            sb.AppendLine("        {");
            foreach (var (t, n) in properties)
            {
                sb.AppendLine($"            public {t} {n} {{ get; set; }}");
            }
            sb.AppendLine("        }");
            sb.AppendLine();
            return sb.ToString();
            
        }
        private static void AddParameters(StringBuilder source, ServiceInfo s)
        {
            var isContainsQuery = IsContainsQuery(s);
            if (s.IsCommandService && !isContainsQuery)
            {
                source.AppendLine($"                {s.RequestName} {GetRequestTypeEndpointName(s.RequestName)},");
                return;
            }

            if (isContainsQuery)
            {
                var queryProperties = GetQueryProperties(s).ToList();
                foreach (var p in queryProperties)
                {
                    source.AppendLine($"                {p.PropertyType} {FirstCharToLower(p.Name)},");
                }
                if(queryProperties.Count == s.RequestProperties.Count) return;
                var className = GetBodyClassName(s);
                source.AppendLine($"                {className} {FirstCharToLower(className)},");
                return;
            }

            foreach (var p in s.RequestProperties)
            {
                source.AppendLine($"                {p.PropertyType} {FirstCharToLower(p.Name)},");
            }
        }

        private static void AddRequestObject(StringBuilder source, ServiceInfo s)
        {
            var isContainsQuery = IsContainsQuery(s);
            if (s.IsCommandService && !isContainsQuery)
            {
                source.AppendLine($"                {GetRequestTypeEndpointName(s.RequestName)},");
                return;
            }
            var parameters = s.RequestProperties.Select(p => new
            {
                IsQuery = true,
                p.PropertyType,
                p.Name
            }).ToArray();
            if (isContainsQuery)
            {
                var queryProperties = GetQueryProperties(s)
                    .Select(p => new
                    {
                        IsQuery = true,
                        p.PropertyType,
                        p.Name
                    });
                var bodyProperties = GetBodyProperties(s)
                    .Select(p => new
                    {
                        IsQuery = false,
                        p.PropertyType,
                        p.Name
                    });
                parameters = queryProperties.Concat(bodyProperties).ToArray();
            }

            var className = FirstCharToLower(GetBodyClassName(s));
            if (s.IsRequestRecord)
            {
                source.Append($"                new {s.RequestName}(");
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
                source.AppendLine($"                new {s.RequestName}");
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