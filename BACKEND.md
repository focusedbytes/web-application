# FocusedBytes Backend - Technical Guidelines

## Architecture Overview

This document provides detailed guidelines for backend development using Event Sourcing, CQRS, and Domain-Driven Design patterns.

---

## Event Sourcing Implementation

### Event Store Structure

**Database Table:**
```sql
CREATE TABLE EventStore (
    Id BIGSERIAL PRIMARY KEY,
    AggregateId UUID NOT NULL,
    AggregateType VARCHAR(255) NOT NULL,
    EventType VARCHAR(255) NOT NULL,
    EventData JSONB NOT NULL,
    Version INT NOT NULL,
    OccurredOn TIMESTAMP NOT NULL,
    UNIQUE(AggregateId, Version)
);
```

**Key Points:**
- Events are immutable - never update or delete
- Version ensures order and prevents concurrency issues
- EventData stored as JSONB for flexibility
- AggregateId + Version unique constraint for optimistic concurrency

### Domain Events Pattern

**Base Class:**
```csharp
public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; }

    protected DomainEvent()
    {
        EventType = GetType().Name;
    }
}
```

**Event Example:**
```csharp
public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; }
    public string? Phone { get; init; }
    public string HashedPassword { get; init; }
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }

    public UserCreatedEvent(...)
    {
        // Initialize properties
    }
}
```

**Event Naming Convention:**
- Past tense: `UserCreated`, `UserUpdated`, `UserDeleted`
- Specific: `AccountUpdated` not just `Updated`
- Domain-focused: reflects business events

### EventStore Repository

**Responsibilities:**
- Save events to database
- Retrieve events for aggregate reconstruction
- Publish events to EventBus after successful save

**Pattern:**
```csharp
public async Task SaveEventsAsync(
    Guid aggregateId,
    string aggregateType,
    IEnumerable<IDomainEvent> events,
    int expectedVersion,
    CancellationToken cancellationToken = default)
{
    var currentVersion = expectedVersion;
    var eventsList = events.ToList();

    // 1. Save to EventStore
    foreach (var @event in eventsList)
    {
        var eventData = JsonSerializer.Serialize(@event, @event.GetType());
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

    // 2. Publish events to update Read Models
    await _eventBus.PublishAllAsync(eventsList, cancellationToken);
}
```

**Important:**
- Publish events AFTER SaveChanges succeeds
- Use transactions if needed
- Handle concurrency exceptions (version conflicts)

---

## CQRS Pattern

### Command Structure

**Command Definition:**
```csharp
// Commands that return a value
public record CreateUserCommand(
    string? Email,
    string? Phone,
    string Password,
    UserRole Role
) : ICommand<Guid>;

// Commands that don't return a value
public record UpdateUserCommand(
    Guid UserId,
    UserRole Role
) : ICommand;
```

**Command Handler:**
```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly EventStoreRepository _eventStore;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public async Task<Guid> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Creating user with email {Email}", command.Email);

        // 1. Create value objects (validated)
        var email = !string.IsNullOrWhiteSpace(command.Email)
            ? Email.Create(command.Email)
            : null;

        // 2. Create aggregate and raise events
        var userId = Guid.NewGuid();
        var user = User.Create(userId, email, phone, hashedPassword, command.Role);

        // 3. Save events
        await _eventStore.SaveEventsAsync(
            userId,
            nameof(User),
            user.GetUncommittedEvents(),
            expectedVersion: 0,
            cancellationToken);

        // 4. Mark events as committed
        user.MarkEventsAsCommitted();

        _logger.Information("User created successfully {UserId}", userId);
        return userId;
    }
}
```

**Handler Responsibilities:**
- Validate command (FluentValidation)
- Load aggregate from EventStore (if update)
- Execute business logic on aggregate
- Save events
- Log operations

### Query Structure

**Query Definition:**
```csharp
public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    bool IncludeDeleted = false
) : IQuery<UserListResult>;

public record UserListResult(
    List<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
```

**Query Handler:**
```csharp
public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, UserListResult>
{
    private readonly ReadModelDbContext _context;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public async Task<UserListResult> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Getting users page {Page}", query.Page);

        var usersQuery = _context.Users
            .Include(u => u.Account)
            .AsQueryable();

        if (!query.IncludeDeleted)
        {
            usersQuery = usersQuery.Where(u => !u.IsDeleted);
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new UserListResult(users, totalCount, query.Page, query.PageSize, totalPages);
    }
}
```

**Query Handler Responsibilities:**
- Query Read Models ONLY (never Aggregates)
- No business logic (pure data retrieval)
- Map to DTOs
- Log queries

---

## Domain-Driven Design (DDD)

### Aggregate Root Pattern

**Definition:**
- Entry point for all modifications
- Ensures consistency boundary
- Raises Domain Events

**Example:**
```csharp
public class User : AggregateRoot
{
    // State
    public Email? Email { get; private set; }
    public Phone? Phone { get; private set; }
    public HashedPassword? HashedPassword { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    // Private constructor - use factory methods
    internal User() { }

    // Factory method
    public static User Create(...)
    {
        // Validation
        if (email == null && phone == null)
            throw new InvalidOperationException("User must have either email or phone");

        // Create and raise event
        var user = new User();
        user.RaiseEvent(new UserCreatedEvent(...));
        return user;
    }

    // Business methods
    public void UpdateRole(UserRole role)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update deleted user");

        RaiseEvent(new UserUpdatedEvent(Id, role));
    }

    // Event application
    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedEvent e: Apply(e); break;
            case UserUpdatedEvent e: Apply(e); break;
            // ...
        }
    }

    private void Apply(UserCreatedEvent @event)
    {
        Id = @event.UserId;
        Email = /* ... */;
        // Set all properties from event
    }
}
```

**Rules:**
- All setters are `private`
- Use factory methods: `Create()`, `Update()`, etc.
- Validate business rules before raising events
- Apply events to update state
- Throw meaningful exceptions

### Value Objects Pattern

**Definition:**
- Immutable objects defined by their values
- Self-validating
- No identity

**Example:**
```csharp
public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(email.ToLowerInvariant());
    }

    public override string ToString() => Value;
}
```

**Benefits:**
- Validation in one place
- Type safety (can't pass string where Email is needed)
- Immutable

**When to Use:**
- Email, Phone, Password, Money, Address
- Any value that has validation rules
- Any value that should be immutable

---

## Read Models (Projections)

### Structure

**Entities:**
```csharp
public class UserReadModel
{
    public Guid Id { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public AccountReadModel? Account { get; set; }
}

public class AccountReadModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string HashedPassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserReadModel? User { get; set; }
}
```

**DbContext:**
```csharp
public class ReadModelDbContext : DbContext
{
    public DbSet<UserReadModel> Users { get; set; }
    public DbSet<AccountReadModel> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadModel>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsDeleted);

            entity.HasOne(e => e.Account)
                .WithOne(e => e.User)
                .HasForeignKey<AccountReadModel>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

**Design Principles:**
- Denormalized for query performance
- No business logic
- Flat structure when possible
- Indexes on frequently queried fields

### Event Handlers (Projections)

**Pattern:**
```csharp
public class UserEventHandler :
    IEventHandler<UserCreatedEvent>,
    IEventHandler<UserUpdatedEvent>,
    IEventHandler<UserDeletedEvent>
{
    private readonly ReadModelDbContext _context;
    private readonly ILogger<UserEventHandler> _logger;

    public async Task HandleAsync(
        UserCreatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.Information(
            "Projecting UserCreatedEvent for user {UserId}",
            @event.UserId);

        var user = new UserReadModel
        {
            Id = @event.UserId,
            Role = @event.Role.ToString(),
            IsActive = @event.IsActive,
            IsDeleted = false,
            CreatedAt = @event.OccurredOn,
            UpdatedAt = @event.OccurredOn
        };

        var account = new AccountReadModel
        {
            Id = Guid.NewGuid(),
            UserId = @event.UserId,
            Email = @event.Email,
            Phone = @event.Phone,
            HashedPassword = @event.HashedPassword,
            CreatedAt = @event.OccurredOn,
            UpdatedAt = @event.OccurredOn
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.Accounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.Information(
            "UserCreatedEvent projected successfully for user {UserId}",
            @event.UserId);
    }
}
```

**Responsibilities:**
- Listen to Domain Events
- Update Read Models
- Idempotent (can process same event multiple times)
- Log all operations

---

## Validation with FluentValidation

### Command Validation

**Validator:**
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        // At least one identifier required
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) ||
                      !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Either Email or Phone must be provided");

        // Email validation (if provided)
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(255)
                .WithMessage("Email must not exceed 255 characters");
        });

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");

        // Role validation
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role");
    }
}
```

**Registration:**
```csharp
// Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddFluentValidationAutoValidation();
```

**Usage in Controller:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(
    [FromBody] CreateUserRequest request)
{
    // Validation happens automatically via FluentValidation
    var command = new CreateUserCommand(...);
    var userId = await _createUserHandler.HandleAsync(command);
    return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { id = userId });
}
```

---

## Error Handling

### Global Exception Handler

**Implementation:**
```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage))
            ),
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Not Found",
                ex.Message
            ),
            InvalidOperationException ex => (
                StatusCodes.Status400BadRequest,
                "Invalid Operation",
                ex.Message
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred"
            )
        };

        _logger.LogError(
            exception,
            "Request failed: {Method} {Path} - {ErrorType}",
            context.Request.Method,
            context.Request.Path,
            exception.GetType().Name);

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title,
            status,
            detail,
            traceId = context.TraceIdentifier
        }, cancellationToken);

        return true;
    }
}
```

**Registration:**
```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
```

### Custom Exceptions

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
```

---

## Logging with Serilog

### Configuration

```csharp
// Program.cs
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FocusedBytes")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/focusedbytes-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Logging Patterns

**In Handlers:**
```csharp
// Command Handler
_logger.Information(
    "Executing {Command} for user {UserId}",
    nameof(UpdateUserCommand),
    command.UserId);

// Query Handler
_logger.Debug(
    "Querying users: Page={Page} PageSize={PageSize}",
    query.Page,
    query.PageSize);

// Event Handler
_logger.Information(
    "Processing {Event} for aggregate {AggregateId}",
    @event.EventType,
    @event.UserId);

// Errors
_logger.Error(
    exception,
    "Failed to process {Command}: {ErrorMessage}",
    nameof(CreateUserCommand),
    exception.Message);
```

**What to Log:**
- All Commands (Information level)
- All Events (Information level)
- Queries (Debug level)
- Errors (Error level with exception)
- Performance-critical operations (with timing)

---

## Resilience with Polly

### Retry Policy

```csharp
public class EventBus
{
    private readonly IAsyncPolicy _retryPolicy;

    public EventBus(IServiceProvider serviceProvider)
    {
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.Warning(
                        exception,
                        "Event publishing failed. Retry {RetryCount} after {Delay}s",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    public async Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Publish event logic
        });
    }
}
```

---

## Health Checks

### Configuration

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString!,
        name: "postgres",
        tags: new[] { "db", "sql" })
    .AddDbContextCheck<EventStoreDbContext>(
        name: "event-store",
        tags: new[] { "db", "eventstore" })
    .AddDbContextCheck<ReadModelDbContext>(
        name: "read-models",
        tags: new[] { "db", "readmodel" });

// Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

**Endpoints:**
- `/health` - Overall health
- `/health/ready` - Readiness probe (Kubernetes)
- `/health/live` - Liveness probe (Kubernetes)

---

## Testing Strategy

### Unit Tests (xUnit + FluentAssertions)

**Aggregate Tests:**
```csharp
public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldRaiseUserCreatedEvent()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var password = HashedPassword.Create("password123");

        // Act
        var user = User.Create(Guid.NewGuid(), email, null, password, UserRole.User);

        // Assert
        var events = user.GetUncommittedEvents();
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<UserCreatedEvent>();
    }

    [Fact]
    public void UpdateRole_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var user = CreateTestUser();
        user.Delete();

        // Act
        Action act = () => user.UpdateRole(UserRole.Admin);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot update deleted user");
    }
}
```

### Integration Tests

**Command Handler Tests:**
```csharp
public class CreateUserCommandHandlerTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUser()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(_eventStore, _logger);
        var command = new CreateUserCommand(
            "test@example.com",
            null,
            "password123",
            UserRole.User);

        // Act
        var userId = await handler.HandleAsync(command);

        // Assert
        userId.Should().NotBeEmpty();

        var events = await _eventStore.GetEventsAsync(userId);
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<UserCreatedEvent>();
    }
}
```

---

## Performance Considerations

### Event Store Optimization

**Snapshots (Future):**
- Store aggregate state at version N
- Replay only events after snapshot
- Implement when aggregates have >100 events

**Indexes:**
```sql
CREATE INDEX idx_eventstore_aggregateid ON EventStore(AggregateId);
CREATE INDEX idx_eventstore_eventtype ON EventStore(EventType);
CREATE INDEX idx_eventstore_occurredon ON EventStore(OccurredOn);
```

### Read Model Optimization

**Indexes:**
```csharp
modelBuilder.Entity<UserReadModel>(entity =>
{
    entity.HasIndex(e => e.IsDeleted);
    entity.HasIndex(e => e.CreatedAt);
    entity.HasIndex(e => e.Role);
});

modelBuilder.Entity<AccountReadModel>(entity =>
{
    entity.HasIndex(e => e.Email).IsUnique();
    entity.HasIndex(e => e.Phone).IsUnique();
    entity.HasIndex(e => e.LastLoginAt);
});
```

**Caching (Future with Redis):**
```csharp
// Cache frequently accessed data
var cacheKey = $"user:{userId}";
var user = await _cache.GetAsync<UserDto>(cacheKey);

if (user == null)
{
    user = await _context.Users.FindAsync(userId);
    await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));
}
```

---

## Migration Guide

### Creating Migrations

```bash
# Event Store migration
dotnet ef migrations add MigrationName --context EventStoreDbContext --output-dir Infrastructure/Persistence/Migrations/EventStore

# Read Models migration
dotnet ef migrations add MigrationName --context ReadModelDbContext --output-dir Infrastructure/Persistence/Migrations/ReadModels

# Apply migrations
dotnet ef database update --context EventStoreDbContext
dotnet ef database update --context ReadModelDbContext
```

---

## Future Enhancements

### Redis Integration

**Read Model Caching:**
```csharp
public class CachedUserQueryHandler : IQueryHandler<GetUserByIdQuery, UserDetailDto?>
{
    private readonly IDistributedCache _cache;
    private readonly GetUserByIdQueryHandler _innerHandler;

    public async Task<UserDetailDto?> HandleAsync(GetUserByIdQuery query, ...)
    {
        var cacheKey = $"user:{query.UserId}";

        // Try cache first
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.Debug("Cache hit for user {UserId}", query.UserId);
            return JsonSerializer.Deserialize<UserDetailDto>(cached);
        }

        // Fallback to database
        var user = await _innerHandler.HandleAsync(query, cancellationToken);

        if (user != null)
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(user),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                },
                cancellationToken);
        }

        return user;
    }
}
```

### Snapshot Mechanism

**When to Implement:**
- Aggregates have >100 events
- Aggregate reconstruction is slow
- Performance degrades noticeably

**Pattern:**
```csharp
public class SnapshotRepository
{
    public async Task<Snapshot?> GetLatestSnapshotAsync(Guid aggregateId)
    {
        return await _context.Snapshots
            .Where(s => s.AggregateId == aggregateId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync();
    }

    public async Task SaveSnapshotAsync(Snapshot snapshot)
    {
        await _context.Snapshots.AddAsync(snapshot);
        await _context.SaveChangesAsync();
    }
}
```

---

**Last Updated**: 2025-11-15
