using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Domain.Users.Entities;
using FocusedBytes.Api.Domain.Users.ValueObjects;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class UpdateAuthMethodCommandHandler : ICommandHandler<UpdateAuthMethodCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<UpdateAuthMethodCommandHandler> _logger;

    public UpdateAuthMethodCommandHandler(EventStoreRepository eventStore, ILogger<UpdateAuthMethodCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateAuthMethodCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Updating auth method for user {UserId} - Identifier: {Identifier}",
            command.UserId,
            command.Identifier);

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);

            // Find the auth method to determine its type
            var authMethod = user.AuthMethods.FirstOrDefault(a => a.Identifier == command.Identifier);
            if (authMethod == null)
                throw new InvalidOperationException($"Auth method with identifier '{command.Identifier}' not found");

            // Hash password if it's email authentication
            string? processedSecret = command.NewSecret;
            if (authMethod.Type == AuthMethodType.Email && !string.IsNullOrWhiteSpace(command.NewSecret))
            {
                var hashedPassword = HashedPassword.Create(command.NewSecret);
                processedSecret = hashedPassword.Value;
                _logger.LogDebug("Password hashed for email authentication");
            }

            user.UpdateAuthMethod(command.Identifier, processedSecret);

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("Auth method updated successfully for user {UserId}", command.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update auth method for user {UserId}", command.UserId);
            throw;
        }
    }
}
