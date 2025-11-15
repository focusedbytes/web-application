# FocusedBytes - Event Sourcing + CQRS Admin Panel

Навчальна платформа з використанням Event Sourcing + CQRS архітектури.

## Архітектура

### Backend (.NET 9)
- **Event Sourcing** - всі зміни зберігаються як події
- **CQRS** - розділення команд (запису) та запитів (читання)
- **PostgreSQL** - Event Store + Read Models
- **EF Core** - ORM для роботи з базою даних

### Frontend (SvelteKit)
- **SvelteKit 5** - SSR фреймворк
- **TypeScript** - типізація
- **Svelte Runes** - нова реактивна система

## Структура проєкту

```
focusedbytes/
├── backend/                          # .NET Backend
│   ├── Domain/                       # Domain Layer
│   │   └── Users/
│   │       ├── User.cs              # Aggregate Root
│   │       ├── Commands/            # Command definitions
│   │       ├── Events/              # Domain events
│   │       └── ValueObjects/        # Value objects (Email, Phone, etc.)
│   ├── Application/                  # Application Layer
│   │   ├── Common/
│   │   │   ├── CQRS/               # CQRS interfaces
│   │   │   └── EventSourcing/      # Event sourcing base classes
│   │   └── Users/
│   │       ├── CommandHandlers/    # Command handlers
│   │       ├── QueryHandlers/      # Query handlers
│   │       ├── EventHandlers/      # Event handlers
│   │       └── Queries/            # Query definitions
│   ├── Infrastructure/              # Infrastructure Layer
│   │   ├── EventStore/             # Event Store implementation
│   │   ├── ReadModels/             # Read Models (Users, Accounts)
│   │   └── Persistence/            # EF Core migrations
│   ├── Controllers/                 # API Controllers
│   └── Program.cs                   # Application entry point
│
└── frontend/                         # SvelteKit Frontend
    ├── src/
    │   ├── lib/
    │   │   ├── api/                # API client
    │   │   ├── components/         # Reusable components
    │   │   └── types/              # TypeScript types
    │   └── routes/
    │       ├── admin/              # Admin dashboard
    │       │   └── users/          # User management
    │       │       ├── +page.svelte         # Users list
    │       │       ├── create/              # Create user
    │       │       └── [id]/edit/           # Edit user
    └── package.json
```

## Особливості архітектури

### Event Sourcing
Кожна зміна в системі зберігається як подія (Event):
- `UserCreatedEvent`
- `UserUpdatedEvent`
- `AccountUpdatedEvent`
- `UserDeactivatedEvent`
- `UserDeletedEvent`

Події зберігаються в Event Store і використовуються для:
1. Відтворення стану агрегату (User)
2. Аудиту змін
3. Можливості подорожі в часі

### CQRS
Розділення операцій:
- **Commands** (запис) → Event Store → Events → Event Handlers → Read Models
- **Queries** (читання) → Read Models (денормалізовані таблиці)

### Read Models
- `Users` - інформація про користувача (роль, статус)
- `Accounts` - дані для входу (email, phone, пароль, останній вхід)

## Налаштування і запуск

### Передумови
- .NET 9 SDK
- PostgreSQL
- Node.js 18+ (для frontend)

### 1. Налаштування бази даних

Створіть базу даних PostgreSQL:
```sql
CREATE DATABASE focusedbytes;
```

Оновіть connection string у `backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=focusedbytes;Username=postgres;Password=postgres"
  }
}
```

### 2. Застосування міграцій

```bash
cd backend

# Застосувати міграції для Event Store
dotnet ef database update --context EventStoreDbContext

# Застосувати міграції для Read Models
dotnet ef database update --context ReadModelDbContext
```

### 3. Запуск Backend

```bash
cd backend
dotnet run
```

Backend буде доступний на `http://localhost:5000`

API Swagger: `http://localhost:5000/swagger`

### 4. Запуск Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend буде доступний на `http://localhost:5173`

## Використання

### Адмінка

1. **Dashboard**: `http://localhost:5173/admin`
2. **Управління користувачами**: `http://localhost:5173/admin/users`

### Функціонал

- ✅ Створення користувача (email або phone + пароль + роль)
- ✅ Перегляд списку користувачів з пагінацією
- ✅ Редагування користувача (роль, email, phone, пароль)
- ✅ Деактивація/активація користувача
- ✅ Видалення користувача (soft delete)
- ✅ Відображення останнього входу

## API Endpoints

### Users Management

```
GET    /api/admin/users              - Отримати список користувачів
GET    /api/admin/users/{id}         - Отримати користувача за ID
POST   /api/admin/users              - Створити користувача
PUT    /api/admin/users/{id}         - Оновити роль користувача
PATCH  /api/admin/users/{id}/account - Оновити account (email, phone, password)
PATCH  /api/admin/users/{id}/status  - Змінити статус (активний/деактивований)
DELETE /api/admin/users/{id}         - Видалити користувача
```

### Приклад створення користувача

```bash
curl -X POST http://localhost:5000/api/admin/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "password123",
    "role": 1
  }'
```

## Розширення функціоналу

### Додавання нової події

1. Створіть клас події в `Domain/Users/Events/`
2. Додайте обробку події в `User.ApplyEvent()`
3. Створіть Event Handler в `Application/Users/EventHandlers/`
4. Оновіть Read Models за потреби

### Додавання нової команди

1. Створіть Command в `Domain/Users/Commands/`
2. Створіть Command Handler в `Application/Users/CommandHandlers/`
3. Додайте метод до User Aggregate
4. Зареєструйте Handler в `Program.cs`

## Технології

### Backend
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- PostgreSQL (Npgsql)
- BCrypt.Net (хешування паролів)
- Swashbuckle (Swagger/OpenAPI)

### Frontend
- SvelteKit 5
- TypeScript
- Vite
- Svelte Runes (новий реактивний API)

## Майбутні покращення

- [ ] Authentication & Authorization
- [ ] Redis для кешування Read Models
- [ ] Server-Sent Events (SSE) для real-time оновлень
- [ ] Offline-first з Service Workers
- [ ] Курси та навчальні матеріали
- [ ] Прогрес навчання
- [ ] Snapshot mechanism для Event Sourcing

## Ліцензія

Proprietary - All rights reserved. See [LICENSE](LICENSE) file for details.

Copyright (c) 2025 Focused Bytes.
