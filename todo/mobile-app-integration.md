# Mobile Application Integration

## Architecture for Mobile

```
                    Mobile App (React Native / Flutter / Swift / Kotlin)
                                        │
                                   HTTP/REST API
                                        │
                    ┌───────────────────┴───────────────────┐
                    │            Your Backend                │
                    │  (Domain + Application + Infrastructure)│
                    └───────────────────────────────────────┘
```

## Your Backend Already Provides

1. **REST API** - Mobile apps consume same endpoints as web
2. **JSON Responses** - Universal format for all clients
3. **Stateless Auth** - JWT tokens work great for mobile
4. **Result Pattern** - Easy to map to mobile UI states

## Mobile-Specific Benefits

### 1. Offline-First Support

Your Event Sourcing enables powerful offline scenarios:

```
Mobile App
├── Local Event Store (SQLite/Realm)
├── Queue commands when offline
├── Sync events when online
└── Replay to rebuild state
```

```dart
// Flutter/Dart example
class MobileLoginCommand {
  Future<void> execute() async {
    if (!hasNetwork) {
      // Queue for later sync
      await _localQueue.add(LoginCommand(email, password));
      return;
    }

    final response = await _api.post('/auth/login', {
      'credential': email,
      'password': password,
    });

    if (response.isSuccess) {
      await _localStorage.saveToken(response.data['token']);
    }
  }
}
```

### 2. Consistent Business Logic

Mobile doesn't need to duplicate:
- Validation rules (backend enforces)
- Business rules (domain layer handles)
- Security policies (lockout, 2FA, etc.)

### 3. Multiple Mobile Clients

Same API serves all:
- iOS (Swift/SwiftUI)
- Android (Kotlin/Jetpack Compose)
- Cross-platform (React Native/Flutter)
- Tablet variations

## What You'd Add for Mobile

### 1. API Versioning

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    // Mobile apps pin to specific version
}
```

### 2. Push Notifications

```csharp
// New event handler for mobile notifications
public class PushNotificationHandler : IEventHandler<AccountLockedEvent>
{
    public async Task HandleAsync(AccountLockedEvent @event, ...)
    {
        await _pushService.SendAsync(@event.UserId,
            "Security Alert",
            "Your account was locked due to failed login attempts");
    }
}
```

### 3. Device Registration

```csharp
public record RegisterDeviceCommand(
    Guid UserId,
    string DeviceToken,
    DevicePlatform Platform  // iOS, Android
);
```

### 4. Refresh Tokens

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    // Long-lived refresh token for mobile sessions
}
```

## Example: React Native Integration

```typescript
// React Native client
class AuthService {
  async login(email: string, password: string): Promise<LoginResult> {
    const response = await fetch(`${API_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ credential: email, password }),
    });

    if (!response.ok) {
      const error = await response.json();
      if (error.message.includes('Two-factor')) {
        return { requires2FA: true };
      }
      throw new Error(error.message);
    }

    const data = await response.json();
    await SecureStore.setItemAsync('token', data.token);
    return { success: true, userId: data.userId };
  }
}
```

## Key Advantage: Single Source of Truth

All clients (Web, Mobile, CLI, Desktop) share:
- Same domain rules
- Same validation
- Same event history
- Same read models
- Same security policies

No logic duplication, no drift between platforms.

## Implementation Priority

1. **API Versioning** - Essential for mobile app store releases
2. **JWT Authentication** - Already planned, required for mobile
3. **Refresh Tokens** - Mobile sessions need long-lived tokens
4. **Push Notifications** - Enhanced user experience
5. **Device Registration** - Track user devices

## Considerations

- **Rate Limiting** - Mobile might have different limits than web
- **Payload Size** - Mobile data usage concerns
- **Caching Headers** - Help mobile cache responses
- **Error Messages** - Mobile-friendly error codes
- **API Documentation** - OpenAPI/Swagger for mobile team
