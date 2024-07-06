//HintName: DeleteUserServiceExtensions.g.cs
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Microsoft.Extensions.DependencyInjection;
namespace ProjectA.User
{
    public static class DeleteUserServiceExtensions
    {
        private const string Endpoint = "user";

        private static string Method = "DELETE";

        public static Delegate Handler = (
                string[] ids,
                IService<projectA.User.Dto.DeleteUserDto, Result> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                new projectA.User.Dto.DeleteUserDto(ids),
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<projectA.User.Dto.DeleteUserDto, Result>),
            typeof(DeleteUserService),
            [
                new Senlin.Mo.Application.Abstractions.Decorators.UnitOfWork.UnitOfWorkAttribute(),
                new Senlin.Mo.Application.Abstractions.Decorators.Log.LogAttribute(),
            ],
            ServiceLifetime.Transient,
            new EndpointData(Endpoint, Handler, Method)
        );
    }
}
