using FocusedBytes.Api.Domain.Users.Entities;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public record CreateUserRequest(
    string Username,
    UserRole Role,
    string AuthIdentifier,
    AuthMethodType AuthType,
    string? AuthSecret);
