using FocusedBytes.Api.Application.Common.EventSourcing;

namespace FocusedBytes.Api.Domain.Users.Events;

public class UserLastLoginUpdatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public DateTime LastLoginAt { get; init; }

    public UserLastLoginUpdatedEvent(Guid userId, DateTime lastLoginAt)
    {
        UserId = userId;
        LastLoginAt = lastLoginAt;
    }
}
