using FluentValidation;
using FocusedBytes.Api.Application.DTOs;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for CreateUserRequest.
/// Validates user creation input to ensure data integrity and business rules compliance.
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // Either email or phone must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Either email or phone must be provided");

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

        // Phone validation (if provided)
        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone cannot be empty")
                .Matches(@"^\+380\d{9}$")
                .WithMessage("Phone must be in format +380XXXXXXXXX")
                .MaximumLength(20)
                .WithMessage("Phone must not exceed 20 characters");
        });

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters")
            .MaximumLength(100)
            .WithMessage("Password must not exceed 100 characters");

        // Role validation
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid user role");
    }
}
