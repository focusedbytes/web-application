using FluentValidation;
using FocusedBytes.Api.Application.DTOs;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for UpdateUserRequest.
/// Validates user role update requests.
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        // Role validation
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid user role");
    }
}
