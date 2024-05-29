//HintName: ProjectA.User.GetUserNameServiceExtensions.cs
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
namespace ProjectA.User
{
    public static class GetUserNameServiceExtensions
    {
        public static Delegate Handler = (
                string userId, 
                IService<GetUserNameDto, string> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                new GetUserNameDto(userId),
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<GetUserNameDto, string>),
            typeof(GetUserNameService),
            [
                typeof(LogDecorator<,>)
            ]
        );
    }
}
