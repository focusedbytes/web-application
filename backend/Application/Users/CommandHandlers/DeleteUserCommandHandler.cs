using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(EventStoreRepository eventStore, ILogger<DeleteUserCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user {UserId}", command.UserId);

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);

            user.Delete();

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("User {UserId} deleted successfully", command.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user {UserId}", command.UserId);
            throw;
        }
    }
}
