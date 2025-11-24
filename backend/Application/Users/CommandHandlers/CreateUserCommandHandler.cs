using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
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
            "Creating user with email: {Email}, role: {Role}",
            command.Email,
            command.Role);

        try
        {
            var email = Email.Create(command.Email);
            var hashedPassword = HashedPassword.Create(command.Password);

            var userId = Guid.NewGuid();
            _logger.LogDebug("Generated new user ID: {UserId}", userId);

            var user = User.Create(userId, email, hashedPassword, command.Role);
            _logger.LogDebug("User aggregate created successfully");

            await _eventStore.SaveEventsAsync(
                userId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: 0,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("User created successfully with ID: {UserId}", userId);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with email: {Email}",
                command.Email);
            throw;
        }
    }
}
