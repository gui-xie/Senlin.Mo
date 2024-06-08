﻿//HintName: AddUserServiceExtensions.g.cs
using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Domain;
using Microsoft.Extensions.DependencyInjection;
namespace ProjectA.User
{
    public static class AddUserServiceExtensions
    {
        private const string Endpoint = "add-user";

        private static string[] Methods = new []{"POST"};

        public static Delegate Handler = (
                ProjectA.User.AddUserDto addUser,
                IService<ProjectA.User.AddUserDto, Result> service,
                CancellationToken cancellationToken) 
            => service.ExecuteAsync(
                addUser,
                cancellationToken);

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<ProjectA.User.AddUserDto, Result>),
            typeof(AddUserService),
            [
                typeof(UnitOfWorkDecorator<,,>),
                typeof(LogDecorator<,>)
            ],
            ServiceLifetime.Transient,
            new EndpointData(Endpoint, Handler, Methods)
        );
    }
}
