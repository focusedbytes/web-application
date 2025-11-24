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
    IEventHandler<AccountUpdatedEvent>,
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
                Role = @event.Role.ToString(),
                IsActive = @event.IsActive,
                IsDeleted = false,
                CreatedAt = @event.OccurredOn,
                UpdatedAt = @event.OccurredOn
            };

            var account = new AccountReadModel
            {
                Id = Guid.NewGuid(),
                UserId = @event.UserId,
                Email = @event.Email,
                Phone = @event.Phone,
                HashedPassword = @event.HashedPassword,
                CreatedAt = @event.OccurredOn,
                UpdatedAt = @event.OccurredOn
            };

            await _context.Users.AddAsync(user, cancellationToken);
            await _context.Accounts.AddAsync(account, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Read models created successfully for user {UserId}", @event.UserId);
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

                _logger.LogInformation("User read model updated successfully for user {UserId}", @event.UserId);
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

    public async Task HandleAsync(AccountUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling AccountUpdatedEvent for user {UserId}", @event.UserId);

        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == @event.UserId, cancellationToken);

            if (account != null)
            {
                if (@event.Email != null)
                {
                    _logger.LogDebug("Updating email for user {UserId}", @event.UserId);
                    account.Email = @event.Email;
                }

                if (@event.Phone != null)
                {
                    _logger.LogDebug("Updating phone for user {UserId}", @event.UserId);
                    account.Phone = @event.Phone;
                }

                if (@event.HashedPassword != null)
                {
                    _logger.LogDebug("Updating password for user {UserId}", @event.UserId);
                    account.HashedPassword = @event.HashedPassword;
                }

                account.UpdatedAt = @event.OccurredOn;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Account read model updated successfully for user {UserId}", @event.UserId);
            }
            else
            {
                _logger.LogWarning("Account for user {UserId} not found in read model when handling AccountUpdatedEvent", @event.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle AccountUpdatedEvent for user {UserId}", @event.UserId);
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
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == @event.UserId, cancellationToken);

            if (account != null)
            {
                account.LastLoginAt = @event.LastLoginAt;
                account.UpdatedAt = @event.OccurredOn;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Last login updated successfully for user {UserId}", @event.UserId);
            }
            else
            {
                _logger.LogWarning("Account for user {UserId} not found in read model when handling UserLastLoginUpdatedEvent", @event.UserId);
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
