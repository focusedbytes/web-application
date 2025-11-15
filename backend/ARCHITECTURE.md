# FocusedBytes Backend Architecture

## Overview

The FocusedBytes backend implements **Event Sourcing + CQRS + Domain-Driven Design (DDD)** patterns to create a scalable, maintainable, and auditable educational platform.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CLIENT (SvelteKit Frontend)                         │
└───────────────────────────────────┬─────────────────────────────────────────┘
                                    │ HTTP Requests
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER (ASP.NET Core)                         │
├─────────────────────────────────────────────────────────────────────────────┤
│  Controllers/Admin/UsersController.cs                                        │
│    • GET /api/admin/users          → Query                                   │
│    • POST /api/admin/users         → Command                                 │
│    • PUT /api/admin/users/{id}     → Command                                 │
│    • DELETE /api/admin/users/{id}  → Command                                 │
│                                                                               │
│  Middleware:                                                                  │
│    • GlobalExceptionHandler  • Serilog  • CORS  • Health Checks              │
└───────────┬──────────────────────────────────────┬───────────────────────────┘
            │                                      │
         COMMAND                               QUERY
            │                                      │
            ▼                                      ▼
┌─────────────────────────────────┐   ┌──────────────────────────────────────┐
│   APPLICATION LAYER (WRITE)     │   │   APPLICATION LAYER (READ)           │
├─────────────────────────────────┤   ├──────────────────────────────────────┤
│  Command Handlers:               │   │  Query Handlers:                     │
│   • CreateUserCommandHandler     │   │   • GetUsersQueryHandler             │
│   • UpdateUserCommandHandler     │   │   • GetUserByIdQueryHandler          │
│   • UpdateAccountCommandHandler  │   │                                      │
│   • DeleteUserCommandHandler     │   │  Queries:                            │
│   • DeactivateUserCommandHandler │   │   • GetUsersQuery                    │
│                                  │   │   • GetUserByIdQuery                 │
│  Validators (FluentValidation):  │   │                                      │
│   • CreateUserRequestValidator   │   │  Reads from:                         │
│   • UpdateUserRequestValidator   │   │   ReadModelDbContext                 │
│   • UpdateAccountRequestValidator│   │    └─> UserReadModel                 │
│   • UpdateUserStatusReq..Val     │   │    └─> AccountReadModel              │
└─────────────┬───────────────────┘   └──────────────────────────────────────┘
              │                                      ▲
              │                                      │
              ▼                                      │
┌─────────────────────────────────┐                 │
│       DOMAIN LAYER (PURE)        │                 │
├─────────────────────────────────┤                 │
│  Aggregate Root:                 │                 │
│   • User.cs                      │                 │
│      - ApplyEvent()              │                 │
│      - Enforce business rules    │                 │
│                                  │                 │
│  Commands:                       │                 │
│   • CreateUserCommand            │                 │
│   • UpdateUserCommand            │                 │
│   • UpdateAccountCommand         │                 │
│   • DeleteUserCommand            │                 │
│   • DeactivateUserCommand        │                 │
│                                  │                 │
│  Domain Events:                  │                 │
│   • UserCreatedEvent             │                 │
│   • UserUpdatedEvent             │                 │
│   • AccountUpdatedEvent          │                 │
│   • UserDeletedEvent             │                 │
│   • UserDeactivatedEvent         │                 │
│   • UserLastLoginUpdatedEvent    │                 │
│                                  │                 │
│  Value Objects:                  │                 │
│   • Email (validation)           │                 │
│   • Phone (validation)           │                 │
│   • HashedPassword (security)    │                 │
│   • UserRole (enum)              │                 │
└─────────────┬───────────────────┘                 │
              │                                      │
              │ Produces Events                     │
              ▼                                      │
┌─────────────────────────────────────────────────────────────────────────────┐
│                       INFRASTRUCTURE LAYER                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│  ┌──────────────────────┐         ┌───────────────────────────────────┐     │
│  │   EventStoreRepository│         │         EventBus                  │     │
│  │                      │         │                                   │     │
│  │  • SaveAsync()       │────────▶│  • PublishAsync()                 │     │
│  │  • LoadAsync()       │         │  • Dispatches to Event Handlers   │     │
│  └──────────┬───────────┘         └────────────┬──────────────────────┘     │
│             │                                   │                            │
│             │ Saves                             │ Notifies                   │
│             ▼                                   ▼                            │
│  ┌──────────────────────┐         ┌───────────────────────────────────┐     │
│  │  EventStoreDbContext │         │     UserEventHandler              │     │
│  │                      │         │                                   │     │
│  │  • Events (table)    │         │  • Handle(UserCreatedEvent)       │     │
│  │  • Append-only log   │         │  • Handle(UserUpdatedEvent)       │     │
│  │                      │         │  • Handle(AccountUpdatedEvent)    │     │
│  └──────────────────────┘         │  • Handle(UserDeletedEvent)       │     │
│                                   │  • Handle(UserDeactivatedEvent)   │     │
│                                   │                                   │     │
│                                   │  Updates ▼                        │     │
│                                   └───────────────────────────────────┘     │
│                                              │                               │
│                                              │ Updates                       │
│                                              ▼                               │
│                                   ┌───────────────────────────────────┐     │
│                                   │   ReadModelDbContext              │     │
│                                   │                                   │     │
│                                   │  • Users (table)                  │     │
│                                   │  • Accounts (table)               │     │
│                                   │  • Denormalized for queries       │     │
│                                   └───────────────────────────────────┘     │
│                                                                               │
└───────────────────────────────────┬───────────────────────────────────┬─────┘
                                    │                                   │
                                    ▼                                   ▼
                    ┌──────────────────────────────┐   ┌──────────────────────┐
                    │   PostgreSQL Database        │   │   PostgreSQL Database│
                    │   (EventStore)               │   │   (ReadModels)       │
                    │                              │   │                      │
                    │  Events Table:               │   │  Users Table:        │
                    │   - Id                       │   │   - Id               │
                    │   - AggregateId              │   │   - FirstName        │
                    │   - EventType                │   │   - LastName         │
                    │   - EventData (JSON)         │   │   - Email            │
                    │   - CreatedAt                │   │   - Role             │
                    │   - Version                  │   │   - Status           │
                    │                              │   │   - IsDeleted        │
                    │  (Source of Truth)           │   │                      │
                    │  (Immutable)                 │   │  Accounts Table:     │
                    │                              │   │   - UserId           │
                    │                              │   │   - Email            │
                    │                              │   │   - PasswordHash     │
                    │                              │   │   - LastLogin        │
                    │                              │   │                      │
                    │                              │   │  (Eventually         │
                    │                              │   │   Consistent)        │
                    └──────────────────────────────┘   └──────────────────────┘
```

## Core Architectural Patterns

### 1. CQRS (Command Query Responsibility Segregation)

**Separation of reads and writes:**

- **Commands (Write Side)**
  - Modify application state
  - Generate domain events
  - Stored in EventStore
  - Examples: CreateUser, UpdateUser, DeleteUser

- **Queries (Read Side)**
  - Read denormalized data
  - No side effects
  - Fast, optimized for queries
  - Examples: GetUsers, GetUserById

**Benefits:**
- Independent scaling of read/write workloads
- Optimized read models for specific queries
- Simpler, more focused code

### 2. Event Sourcing

**All state changes are stored as immutable events:**

```
User Created → UserCreatedEvent
User Updated → UserUpdatedEvent
User Deleted → UserDeletedEvent
```

**Event Flow:**
```
1. Command arrives (e.g., CreateUserCommand)
2. Aggregate applies business rules
3. Aggregate produces event (e.g., UserCreatedEvent)
4. Event is saved to EventStore (source of truth)
5. EventBus publishes event
6. EventHandler updates Read Models
7. Query retrieves data from Read Models
```

**Benefits:**
- Complete audit trail of all changes
- Ability to replay events to rebuild state
- Temporal queries ("What was the state at time X?")
- Debugging by replaying events
- Can create new read models from existing events

### 3. Domain-Driven Design (DDD)

**Layered Architecture:**

#### Domain Layer (Pure Business Logic)
- **No dependencies on infrastructure**
- Contains core business rules and concepts
- Components:
  - **Aggregate Root**: `User` (enforces invariants, applies events)
  - **Value Objects**: `Email`, `Phone`, `HashedPassword` (immutable, self-validating)
  - **Domain Events**: Represent what happened in the domain
  - **Commands**: Represent user intentions

#### Application Layer (Orchestration)
- **Command Handlers**: Execute commands, coordinate domain and infrastructure
- **Query Handlers**: Retrieve data from read models
- **Event Handlers**: Update read models when events occur
- **Validators**: Input validation using FluentValidation

#### Infrastructure Layer (External Concerns)
- **EventStore**: Persistence of events
- **ReadModels**: Denormalized data for queries
- **EventBus**: Event distribution mechanism
- **Database Contexts**: EF Core contexts

#### Presentation Layer (HTTP API)
- **Controllers**: HTTP endpoints
- **Middleware**: Exception handling, logging, CORS
- **Health Checks**: Application monitoring

## Directory Structure

```
backend/
├── Controllers/              # HTTP API endpoints
│   └── Admin/
│       └── UsersController.cs
│
├── Domain/                   # Pure business logic (NO dependencies)
│   └── Users/
│       ├── User.cs          # Aggregate Root
│       ├── Commands/        # User intentions
│       ├── Events/          # What happened
│       └── ValueObjects/    # Email, Phone, etc.
│
├── Application/              # Application logic
│   └── Users/
│       ├── CommandHandlers/ # Execute commands
│       ├── QueryHandlers/   # Execute queries
│       ├── EventHandlers/   # Update read models
│       ├── Queries/         # Query definitions
│       └── Validators/      # FluentValidation
│
├── Infrastructure/           # External concerns
│   ├── EventStore/          # Event persistence
│   │   ├── EventStoreDbContext.cs
│   │   ├── EventStoreRepository.cs
│   │   └── EventBus.cs
│   ├── ReadModels/          # Query models
│   │   ├── ReadModelDbContext.cs
│   │   └── Entities/
│   ├── Middleware/          # GlobalExceptionHandler
│   └── Persistence/
│       └── Migrations/
│
└── Program.cs               # Application startup
```

## Key Components Explained

### Aggregate Root: User

The `User` aggregate is the consistency boundary for user-related operations.

**Responsibilities:**
- Enforce business rules (e.g., email must be unique)
- Apply events to update internal state
- Validate commands before producing events

**Example:**
```csharp
public class User
{
    private Guid _id;
    private string _firstName;
    private Email _email;

    public void ApplyEvent(UserCreatedEvent @event)
    {
        _id = @event.UserId;
        _firstName = @event.FirstName;
        _email = new Email(@event.Email);
    }
}
```

### Value Objects

**Immutable objects defined by their attributes, not identity:**

- `Email`: Validates email format
- `Phone`: Validates phone format
- `HashedPassword`: Securely hashes passwords
- `UserRole`: Enum (Student, Teacher, Admin)

**Benefits:**
- Self-validating
- Cannot be in invalid state
- Reusable across aggregates

### EventStore

**Append-only log of all domain events:**

**Schema:**
```sql
Events
  - Id (GUID)
  - AggregateId (GUID) -- Which user?
  - EventType (string) -- "UserCreatedEvent"
  - EventData (JSONB) -- Event payload
  - CreatedAt (timestamp)
  - Version (int) -- Optimistic concurrency
```

**Operations:**
- `SaveAsync()`: Append events to the log
- `LoadAsync()`: Retrieve all events for an aggregate
- Events are NEVER updated or deleted

### Read Models

**Denormalized data optimized for queries:**

**Schema:**
```sql
Users
  - Id, FirstName, LastName, Email, Role, Status, IsDeleted

Accounts
  - UserId, Email, PasswordHash, LastLogin
```

**Updated by Event Handlers:**
- When `UserCreatedEvent` occurs → Insert into Users table
- When `UserUpdatedEvent` occurs → Update Users table
- When `UserDeletedEvent` occurs → Mark IsDeleted = true

**Eventually Consistent:**
- There might be a brief delay between command execution and read model update
- Acceptable for most use cases

### EventBus

**Distributes events to interested handlers:**

```csharp
public class EventBus
{
    public async Task PublishAsync(object @event)
    {
        // Find all handlers for this event type
        // Call each handler
    }
}
```

**In Production:**
- Could use message queue (RabbitMQ, Azure Service Bus)
- Currently in-process for simplicity

## Request Flow Examples

### Command Flow: Create User

```
1. POST /api/admin/users
   ↓
2. UsersController.CreateUser()
   ↓
3. CreateUserCommandHandler.HandleAsync()
   ↓
4. User.Create() [Domain Logic]
   ↓ (produces)
5. UserCreatedEvent
   ↓
6. EventStoreRepository.SaveAsync()
   → Saves to Events table
   ↓
7. EventBus.PublishAsync(UserCreatedEvent)
   ↓
8. UserEventHandler.Handle(UserCreatedEvent)
   → Inserts into Users table (Read Model)
   ↓
9. Response: { "id": "..." }
```

### Query Flow: Get Users

```
1. GET /api/admin/users?page=1&pageSize=20
   ↓
2. UsersController.GetUsers()
   ↓
3. GetUsersQueryHandler.HandleAsync(GetUsersQuery)
   ↓
4. ReadModelDbContext.Users.Where(...).ToListAsync()
   ↓
5. Response: { items: [...], totalCount: 100 }
```

## Why This Architecture?

### Benefits

✅ **Complete Audit Trail**
- Every change is recorded as an event
- Can answer "who changed what, when, and why?"
- Regulatory compliance (GDPR, SOX, etc.)

✅ **Temporal Queries**
- "What was the user's email on January 1st?"
- Replay events up to a specific point in time

✅ **Scalability**
- Read and write databases can scale independently
- Read replicas for heavy query workloads
- Event store can be partitioned by aggregate

✅ **Performance**
- Denormalized read models = fast queries
- No complex joins needed
- Optimized indexes for specific queries

✅ **Flexibility**
- Add new read models without changing write side
- Create projections for analytics, reporting
- Rebuild read models from events if corrupted

✅ **Debugging**
- Reproduce bugs by replaying events
- Unit test aggregates by checking produced events
- Integration test by verifying event handlers

✅ **Business Alignment**
- Events use domain language (UserRegistered, CourseCompleted)
- Easier communication between developers and domain experts
- Code reflects business processes

### Trade-offs

⚠️ **Complexity**
- More code than traditional CRUD
- Requires understanding of Event Sourcing concepts
- Steeper learning curve for new developers

⚠️ **Eventual Consistency**
- Read models are updated asynchronously
- Brief delay between command and query
- Must handle in UI (e.g., optimistic updates)

⚠️ **Event Schema Evolution**
- Changing event structure requires versioning
- Old events must still be processable
- Migration strategies needed

⚠️ **Storage Growth**
- Events are never deleted (by design)
- Storage grows continuously
- Mitigation: Snapshots, archiving

## Best Practices

### 1. Keep Aggregates Small
- User aggregate handles user data only
- Don't load entire object graph
- Consider separate aggregates for loosely related data

### 2. Events Should Be Immutable
- Never change event structure after publishing
- Use versioning for event schema evolution
- Store events in JSON for flexibility

### 3. Domain Layer Should Be Pure
- No dependencies on EF Core, ASP.NET, etc.
- Easy to unit test
- Portable to other platforms

### 4. Use Value Objects
- Wrap primitives (string email → Email)
- Validation happens in constructor
- Impossible to create invalid value objects

### 5. Keep Event Handlers Simple
- Only update read models
- No complex business logic
- Idempotent (safe to replay)

### 6. Log Everything
- Use Serilog for structured logging
- Log all commands, events, queries
- Invaluable for debugging production issues

## Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core 9
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Health Checks**: ASP.NET Core Health Checks
- **Containerization**: Docker

## Future Enhancements

### Planned
- [ ] Snapshots (optimize aggregate loading)
- [ ] Redis caching for Read Models
- [ ] JWT Authentication & Authorization
- [ ] AutoMapper (Domain ↔ DTOs)
- [ ] Distributed EventBus (RabbitMQ)

### Possible
- [ ] CQRS + Event Sourcing framework (Marten)
- [ ] Outbox pattern (reliable event publishing)
- [ ] Event versioning strategy
- [ ] Saga pattern for long-running processes
- [ ] Read model projections for analytics

## Learning Resources

- [Event Sourcing by Martin Fowler](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design Book](https://www.domainlanguage.com/ddd/)
- [Event Sourcing in .NET](https://github.com/EventStore/EventStore)

## Contributing

When adding new features, follow this pattern:

1. **Define Domain Event** (Domain/Users/Events/)
2. **Create Command** (Domain/Users/Commands/)
3. **Update Aggregate** (Domain/Users/User.cs)
4. **Create Command Handler** (Application/Users/CommandHandlers/)
5. **Create Event Handler** (Application/Users/EventHandlers/)
6. **Update Read Model** (Infrastructure/ReadModels/)
7. **Add Validation** (Application/Validators/)
8. **Create API Endpoint** (Controllers/)

---

**Last Updated**: 2025-11-15
**Architecture Version**: 1.0
**Maintained By**: FocusedBytes Team
