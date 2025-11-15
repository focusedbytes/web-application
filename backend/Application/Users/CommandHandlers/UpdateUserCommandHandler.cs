using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(EventStoreRepository eventStore, ILogger<UpdateUserCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user {UserId} with new role: {Role}", command.UserId, command.Role);

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);
            _logger.LogDebug("User aggregate reconstructed from history");

            user.UpdateRole(command.Role);

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("User {UserId} updated successfully to role: {Role}", command.UserId, command.Role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", command.UserId);
            throw;
        }
    }
}
