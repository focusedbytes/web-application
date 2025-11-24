using FocusedBytes.Api.Application.Common.EventSourcing;

namespace FocusedBytes.Api.Domain.Users.Events;

public class AccountUpdatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string? Email { get; init; }
    public string? HashedPassword { get; init; }

    public AccountUpdatedEvent(
        Guid userId,
        string? email = null,
        string? hashedPassword = null)
    {
        UserId = userId;
        Email = email;
        HashedPassword = hashedPassword;
    }
}
