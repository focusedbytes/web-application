using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(EventStoreRepository eventStore, ILogger<UpdateProfileCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Updating profile for user {UserId} - DisplayName: {HasDisplayName}",
            command.UserId,
            !string.IsNullOrWhiteSpace(command.DisplayName));

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);

            user.UpdateProfile(command.DisplayName);

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("Profile updated successfully for user {UserId}", command.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update profile for user {UserId}", command.UserId);
            throw;
        }
    }
}
