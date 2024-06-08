//HintName: UpdateUserServiceExtensions.g.cs
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Microsoft.Extensions.DependencyInjection;
namespace projectA.User
{
    public static class UpdateUserServiceExtensions
    {
        private const string Endpoint = "user/{id}";

        private static string[] Methods = new []{"PUT"};

       private class UpdateUserDto0
        {
            public string Name { get; set; }
        }

        public static Delegate Handler = (
                string id,
                UpdateUserDto0 updateUserDto0,
                IService<projectA.User.UpdateUserDto, Result> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                new projectA.User.UpdateUserDto
                {
                    Id = id,
                    Name = updateUserDto0.Name
                },
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<projectA.User.UpdateUserDto, Result>),
            typeof(UpdateUserService),
            [
                typeof(UnitOfWorkDecorator<,,>),
                typeof(LogDecorator<,>)
            ],
            ServiceLifetime.Transient,
            new EndpointData(Endpoint, Handler, Methods)
        );
    }
}
