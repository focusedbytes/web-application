using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Domain.Users.Events;
using FocusedBytes.Api.Infrastructure.ReadModels;
using FocusedBytes.Api.Infrastructure.ReadModels.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.EventHandlers;

public class UserEventHandler :
    IEventHandler<UserCreatedEvent>,
    IEventHandler<UserUpdatedEvent>,
    IEventHandler<UserProfileUpdatedEvent>,
    IEventHandler<AuthMethodAddedEvent>,
    IEventHandler<AuthMethodUpdatedEvent>,
    IEventHandler<AuthMethodRemovedEvent>,
    IEventHandler<UserDeactivatedEvent>,
    IEventHandler<UserLastLoginUpdatedEvent>,
    IEventHandler<UserDeletedEvent>
{
    private readonly ReadModelDbContext _context;
    private readonly ILogger<UserEventHandler> _logger;

    public UserEventHandler(ReadModelDbContext context, ILogger<UserEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UserCreatedEvent for user {UserId}", @event.UserId);

        try
        {
            var user = new UserReadModel
            {
                Id = @event.UserId,
                Username = @event.Username,
                Role = @event.Role.ToString(),
                IsActive = true,
                IsDeleted = false,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.OccurredOn
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User read model created successfully for user {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserCreatedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(UserUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UserUpdatedEvent for user {UserId}", @event.UserId);

        try
        {
            var user = await _context.Users.FindAsync(new object[] { @event.UserId }, cancellationToken);
            if (user != null)
            {
                user.Role = @event.Role.ToString();
                user.UpdatedAt = @event.OccurredOn;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User role updated successfully for user {UserId}", @event.UserId);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found in read model when handling UserUpdatedEvent", @event.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserUpdatedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(UserProfileUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UserProfileUpdatedEvent for user {UserId}", @event.UserId);

        try
        {
            var user = await _context.Users.FindAsync(new object[] { @event.UserId }, cancellationToken);
            if (user != null)
            {
                user.DisplayName = @event.DisplayName;
                user.UpdatedAt = @event.OccurredOn;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User profile updated successfully for user {UserId}", @event.UserId);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found in read model when handling UserProfileUpdatedEvent", @event.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserProfileUpdatedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(AuthMethodAddedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling AuthMethodAddedEvent for user {UserId} - Type: {Type}, Identifier: {Identifier}",
            @event.UserId,
            @event.Type,
            @event.Identifier);

        try
        {
            var authMethod = new AuthMethodReadModel
            {
                Id = Guid.NewGuid(),
                UserId = @event.UserId,
                Identifier = @event.Identifier,
                Type = @event.Type.ToString(),
                Secret = @event.Secret,
                CreatedAt = @event.OccurredOn
            };

            await _context.AuthMethods.AddAsync(authMethod, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auth method added successfully for user {UserId}", @event.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle AuthMethodAddedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(AuthMethodUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling AuthMethodUpdatedEvent for user {UserId} - Identifier: {Identifier}",
            @event.UserId,
            @event.Identifier);

        try
        {
            var authMethod = await _context.AuthMethods
                .FirstOrDefaultAsync(a => a.UserId == @event.UserId && a.Identifier == @event.Identifier, cancellationToken);

            if (authMethod != null)
            {
                authMethod.Secret = @event.NewSecret;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Auth method updated successfully for user {UserId}", @event.UserId);
            }
            else
            {
                _logger.LogWarning(
                    "Auth method not found for user {UserId} with identifier {Identifier} when handling AuthMethodUpdatedEvent",
                    @event.UserId,
                    @event.Identifier);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle AuthMethodUpdatedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(AuthMethodRemovedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling AuthMethodRemovedEvent for user {UserId} - Identifier: {Identifier}",
            @event.UserId,
            @event.Identifier);

        try
        {
            var authMethod = await _context.AuthMethods
                .FirstOrDefaultAsync(a => a.UserId == @event.UserId && a.Identifier == @event.Identifier, cancellationToken);

            if (authMethod != null)
            {
                _context.AuthMethods.Remove(authMethod);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Auth method removed successfully for user {UserId}", @event.UserId);
            }
            else
            {
                _logger.LogWarning(
                    "Auth method not found for user {UserId} with identifier {Identifier} when handling AuthMethodRemovedEvent",
                    @event.UserId,
                    @event.Identifier);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle AuthMethodRemovedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(UserDeactivatedEvent @event, CancellationToken cancellationToken = default)
    {
        var action = @event.IsActive ? "activated" : "deactivated";
        _logger.LogInformation("Handling UserDeactivatedEvent for user {UserId} - {Action}", @event.UserId, action);

        try
        {
            var user = await _context.Users.FindAsync(new object[] { @event.UserId }, cancellationToken);
            if (user != null)
            {
                user.IsActive = @event.IsActive;
                user.UpdatedAt = @event.OccurredOn;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} {Action} in read model successfully", @event.UserId, action);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found in read model when handling UserDeactivatedEvent", @event.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserDeactivatedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(UserLastLoginUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UserLastLoginUpdatedEvent for user {UserId}", @event.UserId);

        try
        {
            var user = await _context.Users.FindAsync(new object[] { @event.UserId }, cancellationToken);

            if (user != null)
            {
                user.LastLoginAt = @event.LastLoginAt;
                user.UpdatedAt = @event.OccurredOn;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Last login updated successfully for user {UserId}", @event.UserId);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found in read model when handling UserLastLoginUpdatedEvent", @event.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserLastLoginUpdatedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }

    public async Task HandleAsync(UserDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UserDeletedEvent for user {UserId}", @event.UserId);

        try
        {
            var user = await _context.Users.FindAsync(new object[] { @event.UserId }, cancellationToken);
            if (user != null)
            {
                user.IsDeleted = true;
                user.UpdatedAt = @event.OccurredOn;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} marked as deleted in read model successfully", @event.UserId);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found in read model when handling UserDeletedEvent", @event.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserDeletedEvent for user {UserId}", @event.UserId);
            throw;
        }
    }
}
