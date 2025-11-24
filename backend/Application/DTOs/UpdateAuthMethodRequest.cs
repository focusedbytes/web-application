namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for updating an authentication method's secret (e.g., password, token).
/// </summary>
public record UpdateAuthMethodRequest(
    string Identifier,
    string? NewSecret);
