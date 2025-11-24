namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for updating account credentials.
/// </summary>
public record UpdateAccountRequest(
    string? Email,
    string? Phone,
    string? Password);
