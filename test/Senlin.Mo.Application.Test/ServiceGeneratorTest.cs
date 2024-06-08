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
using ProjectA.Common;
namespace ProjectA.User{
    public class GetUserNameDto{}
    [ServiceEndpoint(""get-user-name"")]
    internal class GetUserNameService: IService<GetUserNameDto, PagedResult<UserName>>
    {
        public Task<string> ExecuteAsync(GetUserNameDto request, CancellationToken cancellationToken)
        {
            return Task.FromResult(""The name is not important."");
        }
    }
    public class UserName{}
}

namespace ProjectA.Common{
    public class PagedResult<T>{
        public int Total{get;set;}
        public IEnumerable<T> Items{get;set;}
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
using projectA.User.Dto;
namespace ProjectA.User{
    [ServiceEndpoint(""user/{id}"", ""DELETE"")]
    internal class DeleteUserService: ICommandService<DeleteUserDto>
    {
        public Task<Result> ExecuteAsync(DeleteUserDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
namespace projectA.User.Dto{
    internal class DeleteUserDto{
        public string Id{get;set;}
    }
}
";
        const string dtoText = @"

";
        var driver = GeneratorDriver(srcText, dtoText);

        var results = driver.GetRunResult();

        return results.Verify();
    }
    
    private static GeneratorDriver GeneratorDriver(params string[] srcTexts)
    {
        var compilation = CreateCompilation(srcTexts);
        ISourceGenerator[] generator = [new ServiceGenerator().AsSourceGenerator()];

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
    
    private static CSharpCompilation CreateCompilation(params string[] srcTexts)
    {
        var syntaxTrees =
            srcTexts.Select(t=> CSharpSyntaxTree.ParseText(t)).ToArray();
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