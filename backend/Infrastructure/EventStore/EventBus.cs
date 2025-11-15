using FocusedBytes.Api.Application.Common.EventSourcing;
using FocusedBytes.Api.Application.Users.EventHandlers;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Infrastructure.EventStore;

public class EventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBus> _logger;

    public EventBus(IServiceProvider serviceProvider, ILogger<EventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Publishing event {EventType} (ID: {EventId})", @event.EventType, @event.EventId);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var eventHandler = scope.ServiceProvider.GetRequiredService<UserEventHandler>();

            await (dynamic)eventHandler.HandleAsync((dynamic)@event, cancellationToken);

            _logger.LogDebug("Event {EventType} (ID: {EventId}) published successfully", @event.EventType, @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} (ID: {EventId})", @event.EventType, @event.EventId);
            throw;
        }
    }

    public async Task PublishAllAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        var eventsList = events.ToList();
        _logger.LogInformation("Publishing {EventCount} event(s) to event handlers", eventsList.Count);

        try
        {
            foreach (var @event in eventsList)
            {
                await PublishAsync(@event, cancellationToken);
            }

            _logger.LogInformation("Successfully published all {EventCount} event(s)", eventsList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish all events");
            throw;
        }
    }
}
