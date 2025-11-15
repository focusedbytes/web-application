namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for updating user activation status.
/// </summary>
public record UpdateUserStatusRequest(bool IsActive);
