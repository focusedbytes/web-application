using FocusedBytes.Api.Application.Common.EventSourcing;

namespace FocusedBytes.Api.Domain.Users.Events;

public class UserProfileUpdatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string? DisplayName { get; init; }

    public UserProfileUpdatedEvent(Guid userId, string? displayName)
    {
        UserId = userId;
        DisplayName = displayName;
    }
}
