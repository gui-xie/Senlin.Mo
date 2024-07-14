//HintName: IGetUserNameService.g.cs
using Senlin.Mo.Application.Abstractions;
namespace ProjectA.User
{
    public interface IGetUserNameService
    {
        Task<ProjectA.Common.PagedResult<ProjectA.User.UserName>> ExecuteAsync(ProjectA.User.GetUserNameDto getUserName, CancellationToken cancellationToken);

        public class GetUserNameServiceImpl : IGetUserNameService
        {
            private readonly IService<ProjectA.User.GetUserNameDto, ProjectA.Common.PagedResult<ProjectA.User.UserName>> _service;

            public GetUserNameServiceImpl(IService<ProjectA.User.GetUserNameDto, ProjectA.Common.PagedResult<ProjectA.User.UserName>> service)
            {
                _service = service;
            }

            public Task<ProjectA.Common.PagedResult<ProjectA.User.UserName>> ExecuteAsync(ProjectA.User.GetUserNameDto getUserName, CancellationToken cancellationToken)
            {
                return _service.ExecuteAsync(getUserName, cancellationToken);
            }
        }

        public static ServiceRegistration Registration = new ServiceRegistration(
            typeof(IService<ProjectA.User.GetUserNameDto, ProjectA.Common.PagedResult<ProjectA.User.UserName>>),
            typeof(GetUserNameServiceImpl),
            [
                new Senlin.Mo.Application.Abstractions.Decorators.UnitOfWork.UnitOfWorkAttribute(),
            ],
           ServiceLifetime.Transient
        );
    }
}
