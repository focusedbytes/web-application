using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Domain.Users.Entities;

namespace FocusedBytes.Api.Domain.Users.Events;

public class AuthMethodAddedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Identifier { get; init; }
    public AuthMethodType Type { get; init; }
    public string? Secret { get; init; }

    public AuthMethodAddedEvent(
        Guid userId,
        string identifier,
        AuthMethodType type,
        string? secret)
    {
        UserId = userId;
        Identifier = identifier;
        Type = type;
        Secret = secret;
    }
}
