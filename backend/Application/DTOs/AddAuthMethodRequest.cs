using FocusedBytes.Api.Domain.Users.Entities;

namespace FocusedBytes.Api.Application.DTOs;

/// <summary>
/// Request DTO for adding an authentication method to a user.
/// </summary>
public record AddAuthMethodRequest(
    string Identifier,
    AuthMethodType Type,
    string? Secret);
