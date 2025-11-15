using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Domain.Users.Events;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users;

public class User : AggregateRoot
{
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public HashedPassword? HashedPassword { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsDeleted { get; private set; }

    internal User() { }

    public static User Create(
        Guid id,
        Email? email,
        Phone? phone,
        HashedPassword hashedPassword,
        UserRole role)
    {
        if (email == null && phone == null)
            throw new InvalidOperationException("User must have either email or phone");

        var user = new User();
        user.RaiseEvent(new UserCreatedEvent(
            id,
            email?.Value ?? string.Empty,
            phone?.Value,
            hashedPassword.Value,
            role,
            isActive: true));

        return user;
    }

    public void UpdateRole(UserRole role)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update deleted user");

        RaiseEvent(new UserUpdatedEvent(Id, role));
    }

    public void UpdateAccount(Email? email = null, Phone? phone = null, HashedPassword? hashedPassword = null)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update deleted user");

        RaiseEvent(new AccountUpdatedEvent(
            Id,
            email?.Value,
            phone?.Value,
            hashedPassword?.Value));
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
            case AccountUpdatedEvent e:
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
        Email = !string.IsNullOrEmpty(@event.Email) ? Email.Create(@event.Email) : null;
        Phone = !string.IsNullOrEmpty(@event.Phone) ? Phone.Create(@event.Phone) : null;
        HashedPassword = HashedPassword.FromHash(@event.HashedPassword);
        Role = @event.Role;
        IsActive = @event.IsActive;
        IsDeleted = false;
    }

    private void Apply(UserUpdatedEvent @event)
    {
        Role = @event.Role;
    }

    private void Apply(AccountUpdatedEvent @event)
    {
        if (@event.Email != null)
            Email = Email.Create(@event.Email);

        if (@event.Phone != null)
            Phone = Phone.Create(@event.Phone);

        if (@event.HashedPassword != null)
            HashedPassword = HashedPassword.FromHash(@event.HashedPassword);
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
