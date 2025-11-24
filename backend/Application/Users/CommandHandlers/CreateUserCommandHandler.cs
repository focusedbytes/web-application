using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Domain.Users.Entities;
using FocusedBytes.Api.Domain.Users.ValueObjects;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(EventStoreRepository eventStore, ILogger<CreateUserCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating user with username: {Username}, auth type: {AuthType}, role: {Role}",
            command.Username,
            command.AuthType,
            command.Role);

        try
        {
            // Hash password if it's email authentication
            string? processedSecret = command.AuthSecret;
            if (command.AuthType == AuthMethodType.Email && !string.IsNullOrWhiteSpace(command.AuthSecret))
            {
                var hashedPassword = HashedPassword.Create(command.AuthSecret);
                processedSecret = hashedPassword.Value;
                _logger.LogDebug("Password hashed for email authentication");
            }

            var userId = Guid.NewGuid();
            _logger.LogDebug("Generated new user ID: {UserId}", userId);

            var user = User.Create(
                userId,
                command.Username,
                command.Role,
                command.AuthIdentifier,
                command.AuthType,
                processedSecret);

            _logger.LogDebug("User aggregate created successfully");

            await _eventStore.SaveEventsAsync(
                userId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: 0,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("User created successfully with ID: {UserId}, Username: {Username}", userId, command.Username);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with username: {Username}",
                command.Username);
            throw;
        }
    }
}
