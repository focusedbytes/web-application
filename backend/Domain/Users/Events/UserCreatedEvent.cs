using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users.Events;

public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string? Phone { get; init; }
    public string HashedPassword { get; init; }
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }

    public UserCreatedEvent(
        Guid userId,
        string email,
        string? phone,
        string hashedPassword,
        UserRole role,
        bool isActive = true)
    {
        UserId = userId;
        Email = email;
        Phone = phone;
        HashedPassword = hashedPassword;
        Role = role;
        IsActive = isActive;
    }
}
