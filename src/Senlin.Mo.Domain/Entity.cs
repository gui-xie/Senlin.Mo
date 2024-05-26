using System.Linq.Expressions;

namespace Senlin.Mo.Domain;

/// <summary>
/// Entity base class
/// </summary>
public abstract class Entity
{
    private List<IDomainEvent>? _domainEvents;

    /// <summary>
    /// Get Domain Events
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        var result = _domainEvents;
        _domainEvents = null;
        return result ?? [];
    }

    /// <summary>
    /// Add Domain Event
    /// </summary>
    /// <param name="e">event</param>
    protected void AddDomainEvent(IDomainEvent e)
    {
        _domainEvents ??= [];
        _domainEvents.Add(e);
    }
}

/// <summary>
///  used for unique judgment
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IUnique<T> where T: class 
{
     /// <summary>
     ///  used for unique judgment
     /// </summary>
     Expression<Func<T, bool>> IsRepeatExpression { get; }
}