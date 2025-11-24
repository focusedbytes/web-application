using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Domain.Common;
using FocusedBytes.Api.Domain.Users.Entities;
using FocusedBytes.Api.Domain.Users.Events;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users;

public class User : AggregateRoot
{
    public string Username { get; private set; } = string.Empty;
    public string? DisplayName { get; private set; }
    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<AuthMethod> _authMethods = new();
    public IReadOnlyCollection<AuthMethod> AuthMethods => _authMethods.AsReadOnly();

    internal User() { }

    public static User Create(
        Guid id,
        string username,
        UserRole role,
        string authIdentifier,
        AuthMethodType authType,
        string? authSecret)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        var user = new User();

        user.RaiseEvent(new UserCreatedEvent(id, username, role, DateTime.UtcNow));

        user.RaiseEvent(new AuthMethodAddedEvent(id, authIdentifier, authType, authSecret));

        return user;
    }

    public void UpdateRole(UserRole role)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update deleted user");

        RaiseEvent(new UserUpdatedEvent(Id, role));
    }

    public void UpdateProfile(string? displayName)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update deleted user");

        RaiseEvent(new UserProfileUpdatedEvent(Id, displayName));
    }

    public void AddAuthMethod(string identifier, AuthMethodType type, string? secret)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify deleted user");

        if (_authMethods.Any(a => a.Identifier == identifier && a.Type == type))
            throw new InvalidOperationException("This auth method is already linked");

        if (type == AuthMethodType.Email && string.IsNullOrWhiteSpace(secret))
            throw new DomainException("Password is required for Email authentication");

        RaiseEvent(new AuthMethodAddedEvent(Id, identifier, type, secret));
    }

    public void UpdateAuthMethod(string identifier, string? newSecret)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify deleted user");

        var method = _authMethods.FirstOrDefault(a => a.Identifier == identifier);
        if (method == null)
            throw new InvalidOperationException("Auth method not found");

        if (method.Type == AuthMethodType.Email && string.IsNullOrWhiteSpace(newSecret))
            throw new DomainException("Password is required for Email authentication");

        RaiseEvent(new AuthMethodUpdatedEvent(Id, identifier, newSecret));
    }

    public void RemoveAuthMethod(string identifier)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify deleted user");

        var method = _authMethods.FirstOrDefault(a => a.Identifier == identifier);
        if (method == null)
            throw new InvalidOperationException("Auth method not found");

        if (_authMethods.Count <= 1)
            throw new InvalidOperationException("Cannot remove the last authentication method");

        RaiseEvent(new AuthMethodRemovedEvent(Id, identifier));
    }

    public void Deactivate()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot deactivate deleted user");

        RaiseEvent(new UserDeactivatedEvent(Id, isActive: false));
    }

    public void Activate()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot activate deleted user");

        RaiseEvent(new UserDeactivatedEvent(Id, isActive: true));
    }

    public void UpdateLastLogin()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update deleted user");

        RaiseEvent(new UserLastLoginUpdatedEvent(Id, DateTime.UtcNow));
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("User is already deleted");

        RaiseEvent(new UserDeletedEvent(Id));
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedEvent e:
                Apply(e);
                break;
            case UserUpdatedEvent e:
                Apply(e);
                break;
            case UserProfileUpdatedEvent e:
                Apply(e);
                break;
            case AuthMethodAddedEvent e:
                Apply(e);
                break;
            case AuthMethodUpdatedEvent e:
                Apply(e);
                break;
            case AuthMethodRemovedEvent e:
                Apply(e);
                break;
            case UserDeactivatedEvent e:
                Apply(e);
                break;
            case UserLastLoginUpdatedEvent e:
                Apply(e);
                break;
            case UserDeletedEvent e:
                Apply(e);
                break;
        }
    }

    private void Apply(UserCreatedEvent @event)
    {
        Id = @event.UserId;
        Username = @event.Username;
        Role = @event.Role;
        IsActive = true;
        IsDeleted = false;
        CreatedAt = @event.CreatedAt;
    }

    private void Apply(UserUpdatedEvent @event)
    {
        Role = @event.Role;
    }

    private void Apply(UserProfileUpdatedEvent @event)
    {
        DisplayName = @event.DisplayName;
    }

    private void Apply(AuthMethodAddedEvent @event)
    {
        _authMethods.Add(new AuthMethod(@event.Identifier, @event.Type, @event.Secret));
    }

    private void Apply(AuthMethodUpdatedEvent @event)
    {
        var method = _authMethods.FirstOrDefault(a => a.Identifier == @event.Identifier);
        method?.UpdateSecret(@event.NewSecret);
    }

    private void Apply(AuthMethodRemovedEvent @event)
    {
        var method = _authMethods.FirstOrDefault(a => a.Identifier == @event.Identifier);
        if (method != null)
            _authMethods.Remove(method);
    }

    private void Apply(UserDeactivatedEvent @event)
    {
        IsActive = @event.IsActive;
    }

    private void Apply(UserLastLoginUpdatedEvent @event)
    {
        LastLoginAt = @event.LastLoginAt;
    }

    private void Apply(UserDeletedEvent @event)
    {
        IsDeleted = true;
    }
}
