using FluentValidation;
using FocusedBytes.Api.Application.DTOs;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for UpdateUserStatusRequest.
/// Validates user status update requests.
/// </summary>
public class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
    public UpdateUserStatusRequestValidator()
    {
        // IsActive is a boolean, so it's always valid
        // No additional validation needed for this simple DTO
        // This validator exists for consistency and future extensibility
    }
}
