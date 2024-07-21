using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Application.Abstractions.Decorators;
using Senlin.Mo.Domain;

namespace Senlin.Mo.Test.Service;

public class ServiceExtensionsTest
{
    [Fact]
    public void GetServiceWithDecoratorSuccess()
    {
        var services = new ServiceCollection();
        var module = new AModule();
        services.AddAppServices(module, []);
        var sp = services.BuildServiceProvider();
        
        var service = sp.GetRequiredService<IService<AModule.AddDto, IResult>>();

        service.Should().BeOfType<LogConsoleService<AModule.AddDto, IResult>>();
    }

    public class AModule : IModule
    {
        public string Name => "A";

        public IEnumerable<ServiceRegistration> GetServices()
        {
            yield return new ServiceRegistration(
                typeof(IService<AddDto, IResult>),
                typeof(AddService),
                [new LogConsoleAttribute("AddService")]
            );
        }

        public Assembly[] Assemblies { get; } = [];

        public record AddDto;

        public class AddService : ICommandService<AddDto>
        {
            public Task<IResult> ExecuteAsync(AddDto request, CancellationToken cancellationToken) => Result.SuccessTask();
        }

        public Type? DbContextType => null;
        public string LocalizationPath => "L";
        public string ConnectionString  => throw new NotImplementedException();
    }

    public class LogConsoleAttribute(string data) : Attribute, IServiceDecorator
    {
        public string Data { get; } = data;
    }

    public class LogConsoleService<TRequest, TResponse>(IService<TRequest, TResponse> service) :
        IService<TRequest, TResponse>, IDecoratorService<LogConsoleAttribute>
    {
        public async Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken)
        {
            var result = await service.ExecuteAsync(request, cancellationToken);
            Console.WriteLine(AttributeData.Data);
            return result;
        }

        public LogConsoleAttribute AttributeData { get; set; } = null!;
    }
}