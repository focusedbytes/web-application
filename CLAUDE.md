# FocusedBytes - AI Assistant Instructions

## Project Overview

**FocusedBytes** is an educational platform designed to help students learn and track their progress through courses. The project is built using modern Event Sourcing + CQRS architecture with Domain-Driven Design principles.

### Core Goals
- Build a scalable, offline-first learning platform
- Implement Event Sourcing + CQRS architecture from scratch (educational purpose)
- Support multilingual/multi-regional users (i18n/l10n planned)
- Maintain clean, understandable code for learning and teaching

### Technology Stack

**Backend (.NET 9)**
- Event Sourcing + CQRS pattern
- Domain-Driven Design (DDD)
- PostgreSQL (Event Store + Read Models)
- EF Core 9
- ASP.NET Core Web API
- Future: Redis for caching Read Models

**Frontend (SvelteKit 5)**
- Server-Side Rendering (SSR)
- Svelte 5 with Runes ($state, $props, $derived)
- Tailwind CSS v4
- TypeScript
- Future: Service Workers for offline-first

---

## Architecture Principles

### Event Sourcing + CQRS

**Core Concept:**
- All changes are stored as immutable events in Event Store
- Read Models are projections updated by Event Handlers
- Commands modify state, Queries read denormalized data

**Flow:**
```
Command → Aggregate → Events → Event Store
                           ↓
                      EventBus
                           ↓
                    Event Handlers → Read Models
                                          ↓
Query → Read Models → Response
```

**Why Not Marten:**
- Learning value: understand fundamentals before using frameworks
- Full control and transparency
- Custom implementation is simple and works well
- Can migrate to Marten later if needed

### Domain-Driven Design

**Structure:**
```
Domain/           # Pure business logic, no dependencies
  └── Users/
      ├── User.cs              # Aggregate Root
      ├── Events/              # Domain Events
      ├── Commands/            # Command definitions
      └── ValueObjects/        # Email, Phone, etc.

Application/      # Application logic, orchestration
  └── Users/
      ├── CommandHandlers/     # Execute commands
      ├── QueryHandlers/       # Execute queries
      └── EventHandlers/       # Update Read Models

Infrastructure/   # External concerns
  ├── EventStore/              # Event persistence
  └── ReadModels/              # Query-optimized models

Controllers/      # API endpoints
```

**Key Patterns:**
- **Aggregate Root**: User - encapsulates business rules
- **Value Objects**: Email, Phone, HashedPassword - immutable, validated
- **Domain Events**: UserCreated, UserUpdated, etc. - what happened
- **Commands**: CreateUser, UpdateUser - what to do
- **Queries**: GetUsers, GetUserById - what to read

---

## Development Philosophy

### Code Quality Standards

1. **Readability Over Cleverness**
   - Code should be self-documenting
   - Clear variable/method names
   - Comments explain WHY, not WHAT

2. **Industrial Best Practices**
   - Follow C# conventions (PascalCase, etc.)
   - Follow TypeScript/Svelte conventions (camelCase variables, PascalCase components)
   - Use `.editorconfig` for consistency

3. **Validation & Error Handling**
   - Use FluentValidation for all input validation
   - GlobalExceptionHandler for centralized error handling
   - Never use bare try-catch or alert()
   - Return meaningful error messages

4. **Logging**
   - Use Serilog extensively
   - Log all Commands, Events, Queries
   - Structured logging: `_logger.Information("UserCreated {UserId} {Email}", userId, email)`

5. **Testing**
   - Backend: xUnit + FluentAssertions + Moq
   - Frontend: Vitest + Testing Library + Playwright
   - Test Commands, Queries, Event Handlers, critical business logic

### Internationalization (i18n/l10n)

**Future Implementation Notes:**
- UI currently in Ukrainian (temporary)
- Will implement i18n for multilingual support
- Database stores data in English
- Consider time zones, date formats, number formats

**For AI Assistants:**
- When generating code, use English for all identifiers, comments, and strings
- Note where i18n will be needed (UI labels, validation messages, etc.)

---

## Git Workflow

### Branching Strategy
```
main                    # Production-ready code
  └── development       # Integration branch
      └── feature/*     # Feature branches
```

### Commit Message Convention

Follow: https://gist.github.com/robertpainsi/b632364184e70900af4ab688decf6f53

**Format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `refactor`: Code refactoring
- `test`: Tests
- `chore`: Build, dependencies

**Examples:**
```
feat(user-admin): add user deactivation feature

Implement user activation/deactivation with domain events.
- Add DeactivateUserCommand
- Add UserDeactivatedEvent
- Update Read Models

Closes #123
```

---

## Communication Preferences

### How I Work with AI Assistants

1. **I'm Learning**
   - Explain WHY, not just HOW
   - Prefer understanding over magic libraries
   - I'll ask "why?" often - be patient

2. **Be Explicit**
   - Show file paths with line numbers
   - Provide complete code examples
   - Explain trade-offs when there are choices

3. **Task Management**
   - Use TodoWrite tool for complex tasks
   - Break down multi-step tasks
   - Mark tasks completed immediately

4. **Questions Welcome**
   - If unclear, ASK before implementing
   - Present options when there are multiple approaches
   - Clarify assumptions

5. **Code Style**
   - Follow established patterns in the codebase
   - Use English for all code, comments, documentation
   - Maintain consistency

### What I Like

- **Transparency**: Show me what you're doing and why
- **Patterns**: Consistent code patterns throughout
- **Learning**: Teach me concepts, not just syntax
- **Pragmatism**: Simple solutions over complex ones

### What I Dislike

- **Magic**: Don't use libraries without explaining
- **Assumptions**: Ask if uncertain about requirements
- **Shortcuts**: Don't skip validation, logging, error handling
- **Inconsistency**: Mixing patterns/styles

---

## Technology Adoption Strategy

### "Adopt NOW" (Current Priority)

**Backend:**
- Serilog (structured logging)
- FluentValidation (validation)
- GlobalExceptionHandler (error handling)
- Polly (resilience & retry)
- HealthChecks (monitoring)
- .editorconfig (code style)

**Frontend:**
- date-fns (date formatting)
- svelte-sonner (toast notifications)
- lucide-svelte (icons)
- bits-ui (dialogs/modals)

### "Adopt WITH Authentication" (Next Phase)

**Backend:**
- JWT Authentication
- AutoMapper (Domain ↔ DTOs)

**Frontend:**
- Zod (form validation)
- Superforms (advanced forms)

### "Adopt LATER" (Future)

**Backend:**
- Redis (caching Read Models)
- Bogus (test data generation)
- Snapshot mechanism (Event Sourcing optimization)

**Frontend:**
- Service Workers (offline-first)
- Server-Sent Events (real-time updates)

**Infrastructure:**
- Docker + Docker Compose
- CI/CD pipelines

---

## Project Status

### Completed
- ✅ Event Sourcing + CQRS infrastructure
- ✅ User management (CRUD with Events)
- ✅ Admin panel UI (SvelteKit + Tailwind)
- ✅ PostgreSQL with EF Core migrations
- ✅ Separation: Users (role, status) + Accounts (credentials, login)

### In Progress
- 🔄 Adding recommended libraries (Serilog, FluentValidation, etc.)
- 🔄 Improving error handling and validation

### Next Priorities
1. Authentication (JWT)
2. Authorization (role-based)
3. Courses management
4. Student progress tracking
5. Testing infrastructure

---

## Important Reminders for AI Assistants

1. **Event Sourcing is Core**
   - Every state change MUST be an Event
   - Never modify Aggregates directly
   - Always publish Events through EventBus

2. **CQRS Separation**
   - Commands go to Aggregates → Events → EventStore
   - Queries go to Read Models only
   - Never query Aggregates in API endpoints

3. **Read Models are Projections**
   - Updated automatically by Event Handlers
   - Denormalized for query performance
   - Eventually consistent (acceptable)

4. **Domain is King**
   - Business rules in Domain layer
   - No infrastructure dependencies in Domain
   - Use Value Objects for validation

5. **Solo Developer**
   - I work alone (for now)
   - Documentation is for future me and future AI assistants
   - Keep it practical and actionable

---

## Questions to Ask Before Major Changes

1. Is this aligned with Event Sourcing + CQRS?
2. Does it follow DDD patterns?
3. Is it consistent with existing code style?
4. Will it help me learn or just hide complexity?
5. Is it the simplest solution that works?

---

## Useful Resources

- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design](https://www.domainlanguage.com/ddd/)
- [Commit Messages](https://gist.github.com/robertpainsi/b632364184e70900af4ab688decf6f53)

---

**Last Updated**: 2025-11-15
**AI Assistant**: Claude (Anthropic)
