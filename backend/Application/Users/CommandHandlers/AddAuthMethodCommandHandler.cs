using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Domain.Users.Entities;
using FocusedBytes.Api.Domain.Users.ValueObjects;
using FocusedBytes.Api.Infrastructure.EventStore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Application.Users.CommandHandlers;

public class AddAuthMethodCommandHandler : ICommandHandler<AddAuthMethodCommand>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<AddAuthMethodCommandHandler> _logger;

    public AddAuthMethodCommandHandler(EventStoreRepository eventStore, ILogger<AddAuthMethodCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(AddAuthMethodCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adding auth method for user {UserId} - Type: {AuthType}, Identifier: {Identifier}",
            command.UserId,
            command.Type,
            command.Identifier);

        try
        {
            var events = await _eventStore.GetEventsAsync(command.UserId, cancellationToken);
            _logger.LogDebug("Loaded {EventCount} events for user {UserId}", events.Count, command.UserId);

            var user = new User();
            user.LoadFromHistory(events);

            // Hash password if it's email authentication
            string? processedSecret = command.Secret;
            if (command.Type == AuthMethodType.Email && !string.IsNullOrWhiteSpace(command.Secret))
            {
                var hashedPassword = HashedPassword.Create(command.Secret);
                processedSecret = hashedPassword.Value;
                _logger.LogDebug("Password hashed for email authentication");
            }

            user.AddAuthMethod(command.Identifier, command.Type, processedSecret);

            await _eventStore.SaveEventsAsync(
                command.UserId,
                nameof(User),
                user.GetUncommittedEvents(),
                expectedVersion: user.Version - user.GetUncommittedEvents().Count,
                cancellationToken);

            user.MarkEventsAsCommitted();

            _logger.LogInformation("Auth method added successfully for user {UserId}", command.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add auth method for user {UserId}", command.UserId);
            throw;
        }
    }
}
