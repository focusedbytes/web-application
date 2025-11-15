using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public record CreateUserRequest(
    string? Email,
    string? Phone,
    string Password,
    UserRole Role);
