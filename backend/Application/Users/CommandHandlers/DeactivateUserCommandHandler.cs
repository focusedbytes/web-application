using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<DeactivateUserCommandHandler> _logger;

    public DeactivateUserCommandHandler(EventStoreRepository eventStore, ILogger<DeactivateUserCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(DeactivateUserCommand command, CancellationToken cancellationToken = default)
    {
        var action = command.IsActive ? "Activating" : "Deactivating";
        _logger.LogInformation("{Action} user {UserId}", action, command.UserId);

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);

            if (command.IsActive)
                user.Activate();
            else
                user.Deactivate();

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("User {UserId} {Action} successfully", command.UserId, action.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to {Action} user {UserId}", action.ToLower(), command.UserId);
            throw;
        }
    }
}
