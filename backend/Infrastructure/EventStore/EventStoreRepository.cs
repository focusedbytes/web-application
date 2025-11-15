using System.Text.Json;
using FocusedBytes.Api.Application.Common.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FocusedBytes.Api.Infrastructure.EventStore;

public class EventStoreRepository
{
    private readonly EventStoreDbContext _context;
    private readonly EventBus _eventBus;
    private readonly ILogger<EventStoreRepository> _logger;

    public EventStoreRepository(EventStoreDbContext context, EventBus eventBus, ILogger<EventStoreRepository> logger)
    {
        _context = context;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task SaveEventsAsync(
        Guid aggregateId,
        string aggregateType,
        IEnumerable<IDomainEvent> events,
        int expectedVersion,
        CancellationToken cancellationToken = default)
    {
        var eventsList = events.ToList();
        _logger.LogInformation(
            "Saving {EventCount} event(s) for aggregate {AggregateType} with ID {AggregateId}, expected version: {ExpectedVersion}",
            eventsList.Count,
            aggregateType,
            aggregateId,
            expectedVersion);

        try
        {
            var currentVersion = expectedVersion;

            foreach (var @event in eventsList)
            {
                var eventData = JsonSerializer.Serialize(@event, @event.GetType());
                _logger.LogDebug(
                    "Serializing event {EventType} (version {Version}) for aggregate {AggregateId}",
                    @event.EventType,
                    currentVersion + 1,
                    aggregateId);

                var eventEntity = new EventStoreEntity
                {
                    AggregateId = aggregateId,
                    AggregateType = aggregateType,
                    EventType = @event.EventType,
                    EventData = eventData,
                    Version = ++currentVersion,
                    OccurredOn = @event.OccurredOn
                };

                await _context.Events.AddAsync(eventEntity, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Events persisted to database successfully");

            // Publish events to update read models
            await _eventBus.PublishAllAsync(eventsList, cancellationToken);
            _logger.LogInformation(
                "Successfully saved and published {EventCount} event(s) for aggregate {AggregateId}",
                eventsList.Count,
                aggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to save events for aggregate {AggregateType} with ID {AggregateId}",
                aggregateType,
                aggregateId);
            throw;
        }
    }

    public async Task<List<IDomainEvent>> GetEventsAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading events for aggregate {AggregateId}", aggregateId);

        try
        {
            var eventEntities = await _context.Events
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => e.Version)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Found {EventCount} event(s) for aggregate {AggregateId}", eventEntities.Count, aggregateId);

            var events = new List<IDomainEvent>();

            foreach (var eventEntity in eventEntities)
            {
                var eventType = Type.GetType($"FocusedBytes.Api.Domain.Users.Events.{eventEntity.EventType}, FocusedBytes.Api");
                if (eventType != null)
                {
                    var @event = JsonSerializer.Deserialize(eventEntity.EventData, eventType) as IDomainEvent;
                    if (@event != null)
                    {
                        events.Add(@event);
                        _logger.LogDebug(
                            "Deserialized event {EventType} (version {Version}) for aggregate {AggregateId}",
                            eventEntity.EventType,
                            eventEntity.Version,
                            aggregateId);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to deserialize event {EventType} (version {Version}) for aggregate {AggregateId}",
                            eventEntity.EventType,
                            eventEntity.Version,
                            aggregateId);
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Event type {EventType} not found for aggregate {AggregateId}",
                        eventEntity.EventType,
                        aggregateId);
                }
            }

            _logger.LogInformation(
                "Successfully loaded {EventCount} event(s) for aggregate {AggregateId}",
                events.Count,
                aggregateId);

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load events for aggregate {AggregateId}", aggregateId);
            throw;
        }
    }
}
