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
    [ServiceRoute(""add-user"")]
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
    
    private static GeneratorDriver GeneratorDriver(string srcText)
    {
        var compilation = CSharpCompilation.Create(
            "ProjectA",
            new[]
            {
                CSharpSyntaxTree.ParseText(srcText)
            },
            references: new[]
            {
                MetadataReference.CreateFromFile(typeof(IService<,>).Assembly.Location),
            },
            options: null
        );

        ISourceGenerator[] generator = [new ServiceGenerator().AsSourceGenerator()];

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
    }
}