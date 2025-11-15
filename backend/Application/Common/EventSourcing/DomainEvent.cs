namespace FocusedBytes.Api.Application.Common.EventSourcing;

public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; }

    protected DomainEvent()
    {
        EventType = GetType().Name;
    }
}
