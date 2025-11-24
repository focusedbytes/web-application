using FluentValidation;
using FocusedBytes.Api.Application.DTOs;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for UpdateAccountRequest.
/// Validates account credential update requests.
/// </summary>
public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        // At least one field must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) ||
                      !string.IsNullOrWhiteSpace(x.Password))
            .WithMessage("At least one field (email or password) must be provided");

        // Email validation (if provided)
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email cannot be empty")
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(255)
                .WithMessage("Email must not exceed 255 characters");
        });

        // Password validation (if provided)
        When(x => !string.IsNullOrWhiteSpace(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password cannot be empty")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters");
        });
    }
}
