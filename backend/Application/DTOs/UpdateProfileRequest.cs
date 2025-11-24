namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for updating user profile.
/// </summary>
public record UpdateProfileRequest(
    string? DisplayName);
