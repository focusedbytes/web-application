using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class RemoveAuthMethodCommandHandler : ICommandHandler<RemoveAuthMethodCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<RemoveAuthMethodCommandHandler> _logger;

    public RemoveAuthMethodCommandHandler(EventStoreRepository eventStore, ILogger<RemoveAuthMethodCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(RemoveAuthMethodCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Removing auth method for user {UserId} - Identifier: {Identifier}",
            command.UserId,
            command.Identifier);

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);

            user.RemoveAuthMethod(command.Identifier);

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("Auth method removed successfully for user {UserId}", command.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove auth method for user {UserId}", command.UserId);
            throw;
        }
    }
}
