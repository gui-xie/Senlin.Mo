namespace Senlin.Mo.Application.Abstractions.Decorators.UnitOfWork;

/// <summary>
/// Unit of work service
/// </summary>
public interface IUnitOfWorkService
{
    /// <summary>
    /// Is enable
    /// </summary>
    bool IsEnable { get; set; }
}