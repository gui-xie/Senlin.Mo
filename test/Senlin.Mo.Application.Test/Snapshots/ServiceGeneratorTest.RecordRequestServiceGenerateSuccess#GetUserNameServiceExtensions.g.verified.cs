//HintName: GetUserNameServiceExtensions.g.cs
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Microsoft.Extensions.DependencyInjection;
namespace ProjectA.User
{
    public static class GetUserNameServiceExtensions
    {
        public static Delegate Handler = (
                string userId,
                IService<ProjectA.User.GetUserNameDto, string> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                new ProjectA.User.GetUserNameDto(userId),
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<ProjectA.User.GetUserNameDto, string>),
            typeof(GetUserNameService),
            [
                typeof(LogDecorator<,>)
            ],
            ServiceLifetime.Transient
        );
    }
}
