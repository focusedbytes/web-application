using FocusedBytes.Api.Application.Common.EventSourcing;

namespace FocusedBytes.Api.Domain.Users.Events;

public class AuthMethodRemovedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Identifier { get; init; }

    public AuthMethodRemovedEvent(Guid userId, string identifier)
    {
        UserId = userId;
        Identifier = identifier;
    }
}
