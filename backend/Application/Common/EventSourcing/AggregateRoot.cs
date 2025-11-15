namespace FocusedBytes.Api.Application.Common.EventSourcing;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();

    public Guid Id { get; protected set; }
    public int Version { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    protected void RaiseEvent(IDomainEvent @event)
    {
        _uncommittedEvents.Add(@event);
        ApplyEvent(@event);
        Version++;
    }

    protected abstract void ApplyEvent(IDomainEvent @event);

    public void LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyEvent(@event);
            Version++;
        }
    }
}
