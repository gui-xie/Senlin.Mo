using Senlin.Mo.Application.Abstractions;

namespace Senlin.Mo.Repository.Abstractions;

/// <summary>
/// Repository helper
/// </summary>
public interface IRepositoryHelper : IClock, IUserHelper, ITenantHelper
{
    /// <summary>
    /// generate a new id
    /// </summary>
    NewId NewId { get; }

    /// <summary>
    /// generate a new concurrency token
    /// </summary>
    NewConcurrencyToken NewConcurrencyToken { get; }
}