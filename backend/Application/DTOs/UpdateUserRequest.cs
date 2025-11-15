using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for updating user role.
/// </summary>
public record UpdateUserRequest(UserRole Role);
