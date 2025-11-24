using FluentValidation;
using FocusedBytes.Api.Application.DTOs;
using FocusedBytes.Api.Domain.Users.Entities;

namespace FocusedBytes.Api.Application.Validators;

/// <summary>
/// FluentValidation validator for AddAuthMethodRequest.
/// </summary>
public class AddAuthMethodRequestValidator : AbstractValidator<AddAuthMethodRequest>
{
    public AddAuthMethodRequestValidator()
    {
        // Identifier validation
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Identifier is required")
            .MaximumLength(255)
            .WithMessage("Identifier must not exceed 255 characters");

        // Type validation
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid authentication type");

        // Email-specific validation
        When(x => x.Type == AuthMethodType.Email, () =>
        {
            RuleFor(x => x.Identifier)
                .EmailAddress()
                .WithMessage("Identifier must be a valid email address for Email auth type");

            RuleFor(x => x.Secret)
                .NotEmpty()
                .WithMessage("Password is required for Email authentication")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters")
                .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters");
        });

        // Phone-specific validation
        When(x => x.Type == AuthMethodType.Phone, () =>
        {
            RuleFor(x => x.Identifier)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Identifier must be a valid phone number in E.164 format");
        });
    }
}
