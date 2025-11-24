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
            "Creating user with email: {Email}, phone: {Phone}, role: {Role}",
            command.Email ?? "N/A",
            command.Phone ?? "N/A",
            command.Role);

        try
        {
            var email = !string.IsNullOrWhiteSpace(command.Email)
                ? Email.Create(command.Email)
                : null;

            var phone = !string.IsNullOrWhiteSpace(command.Phone)
                ? Phone.Create(command.Phone)
                : null;

            var hashedPassword = HashedPassword.Create(command.Password);

            var userId = Guid.NewGuid();
            _logger.LogDebug("Generated new user ID: {UserId}", userId);

            var user = User.Create(userId, email, phone, hashedPassword, command.Role);
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
            _logger.LogError(ex, "Failed to create user with email: {Email}, phone: {Phone}",
                command.Email ?? "N/A",
                command.Phone ?? "N/A");
            throw;
        }
    }
}
