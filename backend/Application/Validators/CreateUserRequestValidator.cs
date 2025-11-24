using FluentValidation;
using FocusedBytes.Api.Application.DTOs;
using FocusedBytes.Api.Domain.Users.Entities;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for CreateUserRequest.
/// Validates user creation input to ensure data integrity and business rules compliance.
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // Username validation
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(6)
            .WithMessage("Username must be at least 6 characters")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

        // Role validation
        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid user role");

        // AuthIdentifier validation
        RuleFor(x => x.AuthIdentifier)
            .NotEmpty()
            .WithMessage("Auth identifier is required")
            .MaximumLength(255)
            .WithMessage("Auth identifier must not exceed 255 characters");

        // AuthType validation
        RuleFor(x => x.AuthType)
            .IsInEnum()
            .WithMessage("Invalid authentication type");

        // Email-specific validation
        When(x => x.AuthType == AuthMethodType.Email, () =>
        {
            RuleFor(x => x.AuthIdentifier)
                .EmailAddress()
                .WithMessage("Auth identifier must be a valid email address for Email auth type");

            RuleFor(x => x.AuthSecret)
                .NotEmpty()
                .WithMessage("Password is required for Email authentication")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters");
        });

        // Phone-specific validation
        When(x => x.AuthType == AuthMethodType.Phone, () =>
        {
            RuleFor(x => x.AuthIdentifier)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Auth identifier must be a valid phone number in E.164 format");
        });
    }
}
