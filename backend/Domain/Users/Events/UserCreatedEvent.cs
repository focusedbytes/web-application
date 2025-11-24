using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users.Events;

public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; }
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }

    public UserCreatedEvent(
        Guid userId,
        string username,
        UserRole role,
        DateTime createdAt)
    {
        UserId = userId;
        Username = username;
        Role = role;
        CreatedAt = createdAt;
    }
}
