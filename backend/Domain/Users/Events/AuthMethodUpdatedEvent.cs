using FocusedBytes.Api.Application.Common.EventSourcing;

namespace FocusedBytes.Api.Domain.Users.Events;

public class AuthMethodUpdatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Identifier { get; init; }
    public string? NewSecret { get; init; }

    public AuthMethodUpdatedEvent(Guid userId, string identifier, string? newSecret)
    {
        UserId = userId;
        Identifier = identifier;
        NewSecret = newSecret;
    }
}
