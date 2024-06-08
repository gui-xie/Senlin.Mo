//HintName: GetUserNameServiceExtensions.g.cs
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
                IService<ProjectA.User.GetUserNameDto, ProjectA.Common.PagedResult<ProjectA.User.UserName>> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                new ProjectA.User.GetUserNameDto
                {

                },
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<ProjectA.User.GetUserNameDto, ProjectA.Common.PagedResult<ProjectA.User.UserName>>),
            typeof(GetUserNameService),
            [
                typeof(LogDecorator<,>)
            ],
            ServiceLifetime.Transient,
            new EndpointData(Endpoint, Handler, Methods)
        );
    }
}
