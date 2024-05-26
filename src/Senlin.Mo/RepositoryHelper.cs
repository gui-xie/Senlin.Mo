using Senlin.Mo.Application.Abstractions;
using Senlin.Mo.Repository.Abstractions;

namespace Senlin.Mo;

internal class RepositoryHelper(
    GetNow getNow,
    GetUserId getUserId,
    GetTenant getTenant,
    IdGenerator idGenerator,
    NewConcurrencyToken newConcurrencyToken
) : IRepositoryHelper
{
    public const string SystemTenant = "__";

    public const string AdminUser = "admin";
    
    public GetNow GetNow => getNow;

    public GetUserId GetUserId => getUserId;

    public GetTenant GetTenant => getTenant;

    public bool IsSystemTenant() => getTenant() == SystemTenant;

    public NewId NewId => idGenerator.New;

    public NewConcurrencyToken NewConcurrencyToken => newConcurrencyToken;

    public IsContainsChangeDataCapture IsContainsChangeDataCapture => () => true;
}