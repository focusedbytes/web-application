using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Domain.Users.ValueObjects;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class UpdateAccountCommandHandler : ICommandHandler<UpdateAccountCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<UpdateAccountCommandHandler> _logger;

    public UpdateAccountCommandHandler(EventStoreRepository eventStore, ILogger<UpdateAccountCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateAccountCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Updating account for user {UserId} - Email: {HasEmail}, Password: {HasPassword}",
            command.UserId,
            !string.IsNullOrWhiteSpace(command.Email),
            !string.IsNullOrWhiteSpace(command.Password));

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);

            var email = !string.IsNullOrWhiteSpace(command.Email)
                ? Email.Create(command.Email)
                : null;

            var hashedPassword = !string.IsNullOrWhiteSpace(command.Password)
                ? HashedPassword.Create(command.Password)
                : null;

            user.UpdateAccount(email, hashedPassword);

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("Account updated successfully for user {UserId}", command.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update account for user {UserId}", command.UserId);
            throw;
        }
    }
}
