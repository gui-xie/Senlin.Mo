using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo.Application.Test;

public class ServiceGeneratorTest
{
    [Fact]
    public Task RecordRequestServiceGenerateSuccess()
    {
        const string srcText = @"
using Senlin.Mo.Application.Abstractions;
namespace ProjectA.User{
    internal record GetUserNameDto(string UserId);
    internal class GetUserNameService: IService<GetUserNameDto, string>
    {
        public Task<string> ExecuteAsync(GetUserNameDto request, CancellationToken cancellationToken)
        {
            return Task.FromResult(""The name is not important."");
        }
    }
}
";
        var driver = GeneratorDriver(srcText);

        var results = driver.GetRunResult();

        return results.Verify();
    }
    
    [Fact]
    public Task ClassRequestServiceGenerateSuccess()
    {
        const string srcText = @"
using Senlin.Mo.Application.Abstractions;
namespace ProjectA.User{
    internal class GetUserNameDto{
        public string UserId{get;set;}
    }
    [ServiceEndpoint(""get-user-name"")]
    internal class GetUserNameService: IService<GetUserNameDto, string>
    {
        public Task<string> ExecuteAsync(GetUserNameDto request, CancellationToken cancellationToken)
        {
            return Task.FromResult(""The name is not important."");
        }
    }
}
";
        var driver = GeneratorDriver(srcText);

        var results = driver.GetRunResult();

        return results.Verify();
    }

    [Fact]
    public Task RouteCommandServiceGenerateSuccess()
    {
        const string srcText = @"
using Senlin.Mo.Application.Abstractions;
namespace ProjectA.User{
    internal class AddUserDto{
        public string UserId{get;set;}
    }
    [ServiceEndpoint(""add-user"", ""POST"")]
    internal class AddUserService: ICommandService<AddUserDto>
    {
        public Task<Result> ExecuteAsync(AddUserDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
";
        var driver = GeneratorDriver(srcText);

        var results = driver.GetRunResult();

        return results.Verify();
    }

    [Fact]
    public Task RoutePutCommandServiceGenerateSuccess()
    {
        const string srcText = @"
using Senlin.Mo.Application.Abstractions;
namespace projectA.User{
    internal class UpdateUserDto{
        public string Id{get;set;}
        public string Name{get;set;}
    }
    [ServiceEndpoint(""user/{id}"", ""PUT"")]
    internal class UpdateUserService: ICommandService<UpdateUserDto>
    {
        public Task<Result> ExecuteAsync(UpdateUserDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
}
";
        var driver = GeneratorDriver(srcText);

        var results = driver.GetRunResult();

        return results.Verify();
    }

    [Fact]
    public Task RouteDeleteCommandServiceGenerateSuccess()
    {
        const string srcText = @"
using Senlin.Mo.Application.Abstractions;
namespace ProjectA.User{
    internal class DeleteUserDto{
        public string Id{get;set;}
    }
    [ServiceEndpoint(""user/{id}"", ""DELETE"")]
    internal class DeleteUserService: ICommandService<DeleteUserDto>
    {
        public Task<Result> ExecuteAsync(DeleteUserDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
";
        var driver = GeneratorDriver(srcText);

        var results = driver.GetRunResult();

        return results.Verify();
    }
    
    private static GeneratorDriver GeneratorDriver(string srcText)
    {
        var compilation = CreateCompilation(srcText);
        ISourceGenerator[] generator = [new ServiceGenerator().AsSourceGenerator()];

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
    
    private static CSharpCompilation CreateCompilation(string srcText)
    {
        SyntaxTree[] syntaxTrees =
        [
            CSharpSyntaxTree.ParseText(srcText)
        ];
        var compilation = CSharpCompilation.Create(
            "ProjectA", 
            syntaxTrees, 
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var references = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location));
        compilation = compilation.AddReferences(references);
        compilation = compilation.AddReferences(
            MetadataReference.CreateFromFile(typeof(IService<,>).Assembly.Location)
        );

        return compilation;
    }
}