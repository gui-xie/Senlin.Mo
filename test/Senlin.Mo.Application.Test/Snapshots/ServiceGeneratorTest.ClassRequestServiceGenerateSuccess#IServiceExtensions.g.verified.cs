//HintName: IServiceExtensions.g.cs
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Microsoft.Extensions.DependencyInjection;
namespace ProjectA.User
{
    public static class GetUserNameServiceExtensions
    {
        private const string Endpoint = "get-user-name";

        private static string[] Methods = new []{"GET"};

        public static Delegate Handler = (
                string userId, 
                IService<GetUserNameDto, string> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                new ProjectA.User.GetUserNameDto
                {
                    UserId = userId
                },
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<GetUserNameDto, string>),
            typeof(GetUserNameService),
            [
                typeof(LogDecorator<,>)
            ],
            ServiceLifetime.Transient,
            new ServiceRouteData(Endpoint, Handler, Methods)
        );
    }
}
