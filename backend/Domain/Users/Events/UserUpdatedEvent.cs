using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users.Events;

public class UserUpdatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public UserRole Role { get; init; }

    public UserUpdatedEvent(Guid userId, UserRole role)
    {
        UserId = userId;
        Role = role;
    }
}
