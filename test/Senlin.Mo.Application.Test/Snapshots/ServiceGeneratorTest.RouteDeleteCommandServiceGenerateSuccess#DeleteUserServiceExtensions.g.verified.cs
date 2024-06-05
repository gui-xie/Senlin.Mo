//HintName: DeleteUserServiceExtensions.g.cs
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Microsoft.Extensions.DependencyInjection;
namespace ProjectA.User
{
    public static class DeleteUserServiceExtensions
    {
        private const string Endpoint = "user/{id}";

        private static string[] Methods = new []{"DELETE"};

        public static Delegate Handler = (
                string id,
                IService<DeleteUserDto, Result> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                new ProjectA.User.DeleteUserDto
                {
                    Id = id
                },
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<DeleteUserDto, Result>),
            typeof(DeleteUserService),
            [
                typeof(UnitOfWorkDecorator<,,>),
                typeof(LogDecorator<,>)
            ],
            ServiceLifetime.Transient,
            new EndpointData(Endpoint, Handler, Methods)
        );
    }
}
