namespace Senlin.Mo.Domain;

/// <summary>
/// Entity base class
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; private set; }
    
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