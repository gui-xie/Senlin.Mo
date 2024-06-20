using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Repository.Abstractions;

namespace Senlin.Mo;

internal class RepositoryHelper(
    GetNow getNow,
    GetUserId getUserId,
    GetTenant getTenant,
    IdGenerator idGenerator,
    NewConcurrencyToken newConcurrencyToken,
    GetSystemTenant getSystemTenant
) : IRepositoryHelper
{
    private readonly string _systemTenant = getSystemTenant();

    public GetNow GetNow => getNow;

    public GetUserId GetUserId => getUserId;

    public GetTenant GetTenant => getTenant;

    public bool IsSystemTenant() => getTenant() == _systemTenant;

    public NewId NewId => idGenerator.New;

    public NewConcurrencyToken NewConcurrencyToken => newConcurrencyToken;

    public IsContainsChangeDataCapture IsContainsChangeDataCapture => () => true;
}