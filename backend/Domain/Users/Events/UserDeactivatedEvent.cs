using FocusedBytes.Api.Application.Common.EventSourcing;

namespace FocusedBytes.Api.Domain.Users.Events;

public class UserDeactivatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }

    public UserDeactivatedEvent(Guid userId, bool isActive)
    {
        UserId = userId;
        IsActive = isActive;
    }
}
