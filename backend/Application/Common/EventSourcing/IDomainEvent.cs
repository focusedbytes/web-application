namespace FocusedBytes.Api.Application.Common.EventSourcing;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
