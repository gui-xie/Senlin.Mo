namespace Senlin.Mo.Application.Abstractions;

/// <summary>
/// Unit Of Work Handler
/// </summary>
public interface IUnitOfWorkHandler
{
    /// <summary>
    /// Save Changes
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}