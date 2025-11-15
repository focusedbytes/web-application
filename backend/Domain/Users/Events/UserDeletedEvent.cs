using FocusedBytes.Api.Application.Common.EventSourcing;

namespace FocusedBytes.Api.Domain.Users.Events;

public class UserDeletedEvent : DomainEvent
{
    public Guid UserId { get; init; }

    public UserDeletedEvent(Guid userId)
    {
        UserId = userId;
    }
}
